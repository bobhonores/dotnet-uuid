public static class LibrariesRouterExtensions
{
    public static RouteGroupBuilder MapUUIDNext(this RouteGroupBuilder group) =>
        group.MapDbUuidGenerator(UUIDNext.Uuid.NewSequential);

    public static RouteGroupBuilder MapUUIDNextDatabase(this RouteGroupBuilder group) =>
        group.MapDbUuidGenerator(() => UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SqlServer),
            () => UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql));

    public static RouteGroupBuilder MapMedo(this RouteGroupBuilder group) =>
        group.MapDbUuidGenerator(Medo.Uuid7.NewMsSqlUniqueIdentifier, Medo.Uuid7.NewUuid7().ToGuid);

    public static RouteGroupBuilder MapUuidExtensions(this RouteGroupBuilder group) =>
        group.MapDbUuidGenerator(() => UuidExtensions.Uuid7.Guid());

    public static RouteGroupBuilder MapNGuidv7(this RouteGroupBuilder group) =>
        group.MapDbUuidGenerator(NGuid.GuidHelpers.CreateVersion7);

    public static RouteGroupBuilder MapNGuidv8(this RouteGroupBuilder group) =>
        group.MapDbUuidGenerator(() =>
        {
            var input = BitConverter.GetBytes(DateTimeOffset.UtcNow.Ticks);
            if (input.Length < 16)
            {
                // var originalLength = input.Length;
                Array.Resize(ref input, 16);

                // // Move bytes from the beginning of the array to the end
                // for (var i = originalLength - 1; i >= 0; i--)
                // {
                //     input[i + 16 - originalLength] = input[i];
                // }
            }

            return NGuid.GuidHelpers.CreateVersion8(input);
        });
}