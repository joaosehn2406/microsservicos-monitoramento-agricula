namespace WeatherCollector.Configurations;

public static class FarmCatalog
{
    public static IReadOnlyList<FarmClient> Clients { get; } =
    [
        new(
            "client-1",
            "Cliente 1",
            [
                new(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Fazenda Boa Vista",
                    -26.9187,
                    -49.0660),
                new(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Sítio Vale Verde",
                    -27.1004,
                    -48.9186)
            ]),
        new(
            "client-2",
            "Cliente 2",
            [
                new(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "Fazenda Santa Clara",
                    -27.5949,
                    -48.5482),
                new(
                    Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    "Estância Primavera",
                    -27.8156,
                    -50.3259)
            ]),
        new(
            "client-3",
            "Cliente 3",
            [
                new(
                    Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    "Fazenda Horizonte",
                    -26.3045,
                    -48.8487)
            ])
    ];

    public static int TotalProperties => Clients.Sum(client => client.Properties.Count);
}

public sealed record FarmClient(
    string Id,
    string Name,
    IReadOnlyList<FarmProperty> Properties);

public sealed record FarmProperty(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude);
