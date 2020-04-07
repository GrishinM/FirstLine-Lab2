using System;
using Npoi.Mapper.Attributes;

namespace Lab2
{
    public class Note
    {
        [Column(0)] public string Id { get; set; }

        [Column(1)] public string Name { get; set; }

        [Column(2)] public string Description { get; set; }

        [Column(3)] public string Source { get; set; }

        [Column(4)] public string Target { get; set; }

        [Column(5)] public string ConfidentialityViolation { get; set; }

        [Column(6)] public string IntegrityViolation { get; set; }

        [Column(7)] public string AvailabilityViolation { get; set; }

        [Column(8)] public string FirstInclusion { get; set; }

        [Column(9)] public string LastChange { get; set; }

        public static bool operator ==(Note first, Note second)
        {
            return first.Name == second.Name && first.Description == second.Description && first.Source == second.Source && first.Target == second.Target &&
                   first.ConfidentialityViolation == second.ConfidentialityViolation && first.IntegrityViolation == second.IntegrityViolation &&
                   first.AvailabilityViolation == second.AvailabilityViolation;
        }

        public static bool operator !=(Note first, Note second)
        {
            return !(first == second);
        }
    }
}