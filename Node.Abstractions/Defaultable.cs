namespace Node.Abstractions
{
    public static class Defaultable
    {
        public static Defaultable<T> FromDefault<T>(T @default) => new Defaultable<T>(@default); 
    }
    public record Defaultable<T> 
    {
        public T Default { get; }

        private T? _value;

        public T Value
        {
            get => _value ?? Default;
            set => _value = value;
        }
        public Defaultable(T @default)
        {
            Default = @default;
        }

        public Defaultable<T> Set(T v)
        {
            Value = v;
            return this;
        }
        public void Reset()
        {
            _value = default;
        }
        
        public static implicit operator T(Defaultable<T> d) => d.Value;
    }
}