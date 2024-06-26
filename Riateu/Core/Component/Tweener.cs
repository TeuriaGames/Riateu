using System;
using System.Threading.Tasks;
using MoonWorks.Graphics;
using MoonWorks.Math;
using MoonWorks.Math.Float;

namespace Riateu.TWTween;

public sealed class Tweener : IPoolable<Tweener>
{
    internal Tween TweenComponent;
    internal Action<Tween> OnReadyCallback;
    internal Action<Tween> OnUpdateCallback;
    internal Action<Tween> OnCompletedCallback;

    public bool Active => TweenComponent!.TimeLeft > 0;
    
    
    public Tweener() 
    {
    }


    public Tweener SetEase(Ease.Easer easing) 
    {
        TweenComponent!.Easer = easing;
        return this;
    }

    public Tweener SetDelay(float delay) 
    {
        TweenComponent!.Delay = delay;
        return this;
    }

    public Tweener OnReady(Action<Tween> tween) 
    {
        this.OnReadyCallback = tween;
        return this;
    }

    public Tweener OnUpdate(Action<Tween> tween) 
    {
        this.OnUpdateCallback = tween;
        return this;
    }

    public Tweener OnCompleted(Action<Tween> tween) 
    {
        this.OnCompletedCallback = tween;
        return this;
    }

    public Tweener Play() 
    {
        TweenComponent.Start();
        return this;
    }

    public async Task WaitAsync() 
    {
#if DEBUG
        if (!TweenComponent.Active) 
        {
            throw new Exception("Tween is not running.");
        }
#endif
        await TweenComponent.WaitAsync();
    }

    public Tweener SetMode(Tween.TweenMode mode) 
    {
        TweenComponent.Mode = mode;
        return this;
    }

    public void Destroy() 
    {
        TweenComponent.DetachSelf();
    }

    internal void AddToTween(Action<Tween> tween, float duration) 
    {
        TweenComponent!.Duration = duration;
        TweenComponent.OnReady = t => 
        {
            OnReadyCallback?.Invoke(t);
        };
        TweenComponent.OnProcess = t => 
        {
            tween(t);
            OnUpdateCallback?.Invoke(t);
        };
        TweenComponent.OnEnd = t => 
        {
            OnCompletedCallback?.Invoke(t);
            Pool<Tweener>.Destroy(this);
        };
    }

    public void Created() {}
}

public static class TweenUtils 
{
#region Rotation
    public static Tweener TWRotation(this Entity entity, float targetRotation, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedRotation = entity.Rotation;
        ctx.AddToTween(t => 
        {
            entity.Rotation = MathHelper.Lerp(cachedRotation, targetRotation, t.Value);
        }, duration);
        return ctx;
    }
#endregion
#region GlobalMove
    public static Tweener TWGlobalMove(this Entity entity, Vector2 targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.Position;
        ctx.AddToTween(t => 
        {
            // entity.Position = cachedPosition + (targetPosition - cachedPosition) * t.Value;
            entity.Position = Vector2.Lerp(cachedPosition, targetPosition, t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWGlobalMoveX(this Entity entity, float targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.Position;
        ctx.AddToTween(t => 
        {
            entity.Position = Vector2.Lerp(cachedPosition, new Vector2(targetPosition, entity.PosY), t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWGlobalMoveY(this Entity entity, float targetPosition, float duration)
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.Position;
        ctx.AddToTween(t => 
        {
            entity.Position = Vector2.Lerp(cachedPosition, new Vector2(entity.PosX, targetPosition), t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWGlobalRelativeMove(this Entity entity, Vector2 targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.Position;
        ctx.AddToTween(t => 
        {
            entity.Position = Vector2.Lerp(cachedPosition, cachedPosition + targetPosition, t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWGlobalRelativeMoveX(this Entity entity, float targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.Position;
        ctx.AddToTween(t => 
        {
            entity.Position = Vector2.Lerp(cachedPosition, new Vector2(cachedPosition.X + targetPosition, entity.PosY), t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWGlobalRelativeMoveY(this Entity entity, float targetPosition, float duration)
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.Position;
        ctx.AddToTween(t => 
        {
            entity.Position = Vector2.Lerp(cachedPosition, new Vector2(entity.PosX, cachedPosition.Y + targetPosition), t.Value);
        }, duration);
        return ctx;
    }
#endregion

#region Move
    public static Tweener TWMove(this Entity entity, Vector2 targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.LocalPosition;
        ctx.AddToTween(t => 
        {
            entity.LocalPosition = Vector2.Lerp(cachedPosition, targetPosition, t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWMoveX(this Entity entity, float targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.LocalPosition;
        ctx.AddToTween(t => 
        {
            entity.LocalPosition = Vector2.Lerp(
                cachedPosition, 
                new Vector2(targetPosition, entity.LocalPosition.Y), 
                t.Value
            );
        }, duration);
        return ctx;
    }

    public static Tweener TWMoveY(this Entity entity, float targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.LocalPosition;
        ctx.AddToTween(t => 
        {
            entity.LocalPosition = Vector2.Lerp(
                cachedPosition, 
                new Vector2(entity.LocalPosition.X, targetPosition), 
                t.Value
            );
        }, duration);
        return ctx;
    }

    public static Tweener TWRelativeMove(this Entity entity, Vector2 targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.LocalPosition;
        ctx.AddToTween(t => 
        {
            entity.LocalPosition = Vector2.Lerp(cachedPosition, cachedPosition + targetPosition, t.Value);
        }, duration);
        return ctx;
    }

    public static Tweener TWRelativeMoveX(this Entity entity, float targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.LocalPosition;
        ctx.AddToTween(t => 
        {
            entity.LocalPosition = Vector2.Lerp(
                cachedPosition, 
                new Vector2(cachedPosition.X + targetPosition, entity.LocalPosition.Y), 
                t.Value
            );
        }, duration);
        return ctx;
    }

    public static Tweener TWRelativeMoveY(this Entity entity, float targetPosition, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedPosition = entity.LocalPosition;
        ctx.AddToTween(t => 
        {
            entity.LocalPosition = Vector2.Lerp(
                cachedPosition, 
                new Vector2(entity.LocalPosition.X, cachedPosition.Y + targetPosition), 
                t.Value
            );
        }, duration);
        return ctx;
    }
#endregion

#region Modulate
    public static Tweener TWModulate(this Entity entity, Color targetModulate, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        var cachedColor = entity.Modulate;
        ctx.AddToTween(t => 
        {
            entity.Modulate = Color.Lerp(cachedColor, targetModulate, t.Value);
        }, duration);
        return ctx;
    }

#endregion

    public static Tweener TWCustom(this Entity entity, Action<Tween> onProcess, float duration) 
    {
        var ctx = Pool<Tweener>.Create();
        ctx.TweenComponent = Tween.Create(entity, Tween.TweenMode.OneShot, null, duration);
        ctx.AddToTween(t => 
        {
            onProcess(t);
        }, duration);
        return ctx;
    }
}