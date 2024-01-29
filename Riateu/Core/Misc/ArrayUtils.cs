using System;

namespace Riateu;

public static class ArrayUtils 
{
    public const int NorthWest = 1 << 0;
    public const int North = 1 << 1;
    public const int NorthEast = 1 << 2;
    public const int West = 1 << 3;
    public const int East = 1 << 4;
    public const int SouthWest = 1 << 5;
    public const int South = 1 << 6;
    public const int SouthEast = 1 << 7;

    public static bool ArrayCheck<T>(int x, int y, T[,] grids) 
    {
        if (!(x < grids.GetLength(0) && y < grids.GetLength(1) && x >= 0 && y >= 0)) 
            return false;
        
        return true;
    }

    public static bool ArrayCheck<T>(int x, int y, Array2D<T> grids) 
    {
        if (!(x < grids.Rows && y < grids.Columns && x >= 0 && y >= 0)) 
            return false;
        
        return true;
    }


    public static bool ArrayFilter<T>(int x, int y, T[,] grids, Func<T, bool> checker) 
    {
        if (!(x < grids.GetLength(0) && y < grids.GetLength(1) && x >= 0 && y >= 0)) 
            return false;
        
        if (checker(grids[x, y])) 
            return true;
        
        return false;
    }

    public static bool ArrayFilter<T>(int x, int y, Array2D<T> grids, Func<T, bool> checker) 
    {
        if (!(x < grids.Rows && y < grids.Columns && x >= 0 && y >= 0)) 
            return false;
        
        if (checker(grids[x, y])) 
            return true;
        
        return false;
    }

    public static bool ArrayWrapFilter<T>(int x, int y, T[,] grids, Func<T, bool> checker) 
    {
        if (!(x < grids.GetLength(0) && y < grids.GetLength(1) && x >= 0 && y >= 0)) 
            return true;
        
        if (checker(grids[x, y])) 
            return true;
        
        return false;
    }

    public static bool ArrayWrapFilter<T>(int x, int y, Array2D<T> grids, Func<T, bool> checker) 
    {
        if (!(x < grids.Rows && y < grids.Columns && x >= 0 && y >= 0)) 
            return true;
        
        if (checker(grids[x, y])) 
            return true;
        
        return false;
    }

    public static int GetNeighbourMask<T>(
        int x, int y, T[,] array2D, Func<T, bool> checker = null, bool wrapped = false) 
    {
        Func<int, int, bool> check;
        if (checker == null)
            check = (int x, int y) => ArrayCheck(x, y, array2D);
        else if (wrapped)
            check = (int x, int y) => ArrayWrapFilter(x, y, array2D, checker);
        else
            check = (int x, int y) => ArrayFilter(x, y, array2D, checker);
        int mask = 0;

        if (check(x, y + 1)) mask += East;
        if (check(x, y - 1)) mask += West;
        if (check(x + 1, y)) mask += South;
        if (check(x - 1, y)) mask += North;

        if ((mask & (South | West)) == (South | West) && check(x + 1, y - 1))
            mask += SouthWest;

        if ((mask & (South | East)) == (South | East) && check(x + 1, y + 1))
            mask += SouthEast;

        if ((mask & (North | West)) == (North | West) && check(x - 1, y - 1))
            mask += NorthWest;

        if ((mask & (North | East)) == (North | East) && check(x - 1, y + 1))
            mask += NorthEast;
        return mask;
    }

    public static int GetNeighbourMask<T>(
        int x, int y, Array2D<T> array2D, Func<T, bool> checker = null, bool wrapped = false) 
    {
        Func<int, int, bool> check;
        if (checker == null)
            check = (int x, int y) => ArrayCheck(x, y, array2D);
        else if (wrapped)
            check = (int x, int y) => ArrayWrapFilter(x, y, array2D, checker);
        else
            check = (int x, int y) => ArrayFilter(x, y, array2D, checker);
        int mask = 0;

        if (check(x, y + 1)) mask += South;
        if (check(x, y - 1)) mask += North;
        if (check(x + 1, y)) mask += East;
        if (check(x - 1, y)) mask += West;

        if ((mask & (North | East)) == (North | East) && check(x + 1, y - 1))
            mask += NorthEast;

        if ((mask & (South | East)) == (South | East) && check(x + 1, y + 1))
            mask += SouthEast;

        if ((mask & (North | West)) == (North | West) && check(x - 1, y - 1))
            mask += NorthWest;

        if ((mask & (South | West)) == (South | West) && check(x - 1, y + 1))
            mask += SouthWest;
        return mask;
    }
}