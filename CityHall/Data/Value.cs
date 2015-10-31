namespace CityHall.Data
{
    public class Value
    {
        public string value { get; set; }
        public bool? protect { get; set; }

        public Value() 
        {
            this.value = null;
            this.protect = null;
        }

        public Value(string value)
        {
            this.value = value;
            this.protect = null;
        }

        public Value(bool protect)
        {
            this.value = null;
            this.protect = protect;
        }

        public Value(string value, bool protect)
        {
            this.value = value;
            this.protect = protect;
        }

        internal object ToPayload()
        {
            bool haveValue = this.value != null;
            bool haveProtect = this.protect.HasValue;

            if (haveValue && haveProtect)
            {
                return new { value = this.value, protect = this.protect.Value };
            }
            else if (haveValue && !haveProtect)
            {
                return new { value = this.value };
            }
            else if (!haveValue && haveProtect)
            {
                return new { protect = this.protect.Value };
            }

            return new object();
        }
    }
}
