namespace SchoolERP.Domain.Transport.Enums;

/// <summary>Monthly, Quarterly, HalfYearly — stored as lowercase/snake_case (monthly, quarterly, half_yearly). No annual or one_time per DB check constraint.</summary>
public enum TransportFeeFrequency { Monthly = 1, Quarterly = 2, HalfYearly = 3 }
