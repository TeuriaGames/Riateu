using System.Collections.Concurrent;

namespace Riateu.Graphics;

internal class DrawBatchPool
{
	private ConcurrentQueue<DrawBatch> drawBatches = new ConcurrentQueue<DrawBatch>();

	public DrawBatch Obtain()
	{
		if (drawBatches.TryDequeue(out var drawBatch))
		{
			return drawBatch;
		}
		else
		{
            return new DrawBatch();
        }
	}

	public void Return(DrawBatch drawBatch)
	{
		drawBatches.Enqueue(drawBatch);
	}
}
