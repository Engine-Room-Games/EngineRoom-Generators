namespace EngineRoom.Runtime.Singleton
{
    /// <summary>
    /// Common ancestor for generated singleton interfaces. The static
    /// <see cref="Instance"/> slot is per closed generic, so each generated
    /// I&lt;Foo&gt; gets its own storage.
    /// </summary>
    public interface ISingleton<TInterface>
    {
        public static TInterface Instance { get; protected set; } = default!;
    }
}
