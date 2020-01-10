using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceApi.Helpers
{
    public class RateLimiterType
    {
        int Max { get; set; }
        int PeriodMilliseconds { get; set; }
        double Margin { get; set; }
        int Curent { get; set; }
        DateTime Time { get; set; }

        public RateLimiterType(int max, int periodMilliseconds)
        {
            this.Max = max;
            this.PeriodMilliseconds = periodMilliseconds;
            this.Margin = 0.99d;
        }
        public bool Wait()
        {
            Tick();
            return (Curent >= Max * Margin);
        }
        public void Use()
        {
            Curent++;
        }
        public void UpdateMax(int max)
        {
            Max = max;
        }
        private void Tick()
        {
            if ((DateTime.UtcNow - Time).TotalMilliseconds >= PeriodMilliseconds)
            {
                Reset();
            }
        }
        private void Reset()
        {
            Time = DateTime.UtcNow;
            Curent = 0;
        }
    }

    public class RateLimiter
    {
        ConcurrentDictionary<string, RateLimiterType> RateLimiterTypes = new ConcurrentDictionary<string, RateLimiterType>();

        public RateLimiter()
        {
        }
        public RateLimiter Add(string type, RateLimiterType rateLimiterType)
        {
            lock (this)
            {
                RateLimiterTypes.TryAdd(type, rateLimiterType);
                return this;
            }
        }
        public bool Wait()
        {
            foreach (var rateLimiterType in RateLimiterTypes)
                if (rateLimiterType.Value.Wait())
                    return true;
            return false;
        }
        public void Use(string type)
        {
            lock (this)
            {
                RateLimiterTypes[type].Use();
            }
        }
        public void UpdateMax(string type, int max)
        {
            lock (this)
            {
                RateLimiterTypes[type].UpdateMax(max);
            }
        }
    }
}
