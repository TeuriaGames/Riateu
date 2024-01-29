using System;

namespace Riateu;

public sealed class Array2D<T> 
{
    private T[] array;
    private int numRows;
    private int numColumns;

    public int Rows => numRows;
    public int Columns => numColumns;
    public static Array2D<T> Empty => empty;
    private static readonly Array2D<T> empty = new Array2D<T>();

    public T this[int x, int y] 
    {
        get 
        {
            var index = GetIndex(x, y);
            if (index < 0) 
                throw new IndiciesOutOfBoundsException(x, y);
            return array[GetIndex(x, y)];
        }
        set 
        {
            var index = GetIndex(x, y);
            if (index < 0) 
                throw new IndiciesOutOfBoundsException(x, y);
            array[index] = value;
        }
    }

    internal Array2D() 
    {
        array = Array.Empty<T>();
    }

    public void Fill(T value) 
    {
        Array.Fill(array, value);
    }

    public Array2D(int numRows, int numColumns)  
    {
        this.numRows = numRows;
        this.numColumns = numColumns;
        array = new T[numRows * numColumns];
    }

    public static Array2D<T> FromArray(int numRows, int numColumns, T[] grid) 
    {
        var array = new Array2D<T>(numRows, numColumns);
        array.array = grid;
        return array;
    }

    public static Array2D<T> FromArray2D(T[,] grid2D) 
    {
        var row = grid2D.GetLength(0);
        var column = grid2D.GetLength(1);
        var array = new Array2D<T>(row, column);
        for (int x = 0; x < row; x++) 
            for (int y = 0; y < column; y++) 
                array[x, y] = grid2D[x, y];

        return array;
    }

    private int GetIndex(int row, int column) 
    {
        if (row < this.numRows && column < this.numColumns) 
        {
            return row * this.numColumns + column;
        }
        return -1;
    }

    public T[] AsArray() => array;

    public T[] ToArray() 
    {
        var arr = new T[array.Length];
        Array.Copy(array, arr, array.Length);
        return arr;
    }

    public Array2D<T> Clone()
    {
        var array = new Array2D<T>(numRows, numColumns)
        {
            array = this.array
        };
        return array;
    }

    public void Resize(int rows, int columns) 
    {
        Resize(rows, columns, default(T)!);
    }

    public void Resize(int rows, int columns, T filler) 
    {
        var stackedArray = new StackArray2D<T>(rows, columns);
        stackedArray.Fill(filler);
        int minRows = Math.Min(rows, numRows);
        int minCols = Math.Min(columns, numColumns);
        for (int i = 0; i < minRows; i++)
            for (int j = 0; j < minCols; j++) 
            {
                stackedArray[i, j] = this[i, j];
            }
        
        numRows = rows;
        numColumns = columns;
        array = stackedArray.ToArray();
    }
}

public ref struct StackArray2D<T> 
{
    private Span<T> array;
    private int numRows;
    private int numColumns;

    public int Rows => numRows;
    public int Columns => numColumns;

    public T this[int x, int y] 
    {
        get 
        {
            var index = GetIndex(x, y);
            if (index < 0) 
                throw new IndiciesOutOfBoundsException(x, y);
            return array[GetIndex(x, y)];
        }
        set 
        {
            var index = GetIndex(x, y);
            if (index < 0) 
                throw new IndiciesOutOfBoundsException(x, y);
            array[index] = value;
        }
    }

    public StackArray2D(int numRows, int numColumns)  
    {
        this.numRows = numRows;
        this.numColumns = numColumns;
        array = new T[numRows * numColumns];
    }

    public void Fill(T value) 
    {
        array.Fill(value);
    }

    public static StackArray2D<T> FromArray(int numRows, int numColumns, T[] grid) 
    {
        var array = new StackArray2D<T>(numRows, numColumns);
        array.array = grid;
        return array;
    }

    public static StackArray2D<T> FromArray2D(int numRows, int numColumns, T[,] grid2D) 
    {
        var array = new StackArray2D<T>(numRows, numColumns);
        for (int x = 0; x < numColumns; x++) 
            for (int y = 0; y < numRows; y++) 
                array[x, y] = grid2D[x, y];

        return array;
    }

    private int GetIndex(int row, int column) 
    {
        if (row < this.numRows && column < this.numColumns) 
        {
            return row * this.numColumns + column;
        }
        return -1;
    }

    public T[] ToArray() 
    {
        return array.ToArray();
    }

    public Span<T> ToSpanArray() 
    {
        return array;
    }
}

[Serializable]
public class IndiciesOutOfBoundsException : Exception 
{
    public IndiciesOutOfBoundsException(int indexX, int indexY) 
        : base($"Indicies Out of bounds Exception X: {indexX} Y: {indexY}") {}
}