namespace ButterMorph.SampleSharedTypes;

using ButterMorph.Contracts;

/// <summary>
/// Sample RFC value object.
/// </summary>
[ButterMorphLanguageType("RFC", Name = "RFC", Version = "1.0.0", Description = "Mexican RFC business identifier.")]
public readonly record struct Rfc(string Value);

/// <summary>
/// Sample customer id value object.
/// </summary>
[ButterMorphLanguageType("CustomerId", Name = "Customer Id", Version = "1.0.0", Description = "Stable customer identifier.")]
public readonly record struct CustomerId(string Value);

/// <summary>
/// Sample email value object.
/// </summary>
[ButterMorphLanguageType("EmailAddress", Name = "Email Address", Version = "1.0.0", Description = "Business email address.")]
public readonly record struct EmailAddress(string Value);

/// <summary>
/// Sample money value object.
/// </summary>
[ButterMorphLanguageType("Money", Name = "Money", Version = "1.0.0", Description = "Decimal money amount.")]
public readonly record struct Money(decimal Value);
