using System;

namespace SuperSmashTrees.Core.Models
{
    public enum PowerType
    {
        ForcePush,
        Shield,
        AirJump
    }

    public abstract class Power
    {
        public PowerType Type { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public int Duration { get; protected set; } // Duración en segundos
        public bool IsActive { get; protected set; }
        public DateTime ActivationTime { get; protected set; }

        protected Power(PowerType type, string name, string description, int duration)
        {
            Type = type;
            Name = name;
            Description = description;
            Duration = duration;
            IsActive = false;
        }

        public virtual void Activate()
        {
            IsActive = true;
            ActivationTime = DateTime.Now;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
        }

        public virtual bool IsExpired()
        {
            if (!IsActive)
                return false;

            return (DateTime.Now - ActivationTime).TotalSeconds >= Duration;
        }

        public abstract void ApplyEffect(Player player);
    }
}