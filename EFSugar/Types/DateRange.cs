using System;
using System.Collections.Generic;
using System.Text;

namespace EFSugar
{
    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public bool IsBetween(DateTime date, DateRangeBetweenOptions options = DateRangeBetweenOptions.BothInclusive)
        {
            switch (options)
            {
                case DateRangeBetweenOptions.StartInclusive:
                    return date >= Start && date < End;
                case DateRangeBetweenOptions.EndInclusive:
                    return date > Start && date <= End;
                case DateRangeBetweenOptions.BothInclusive:
                default:
                    return date >= Start && date <= End;
            }
        }
    }

    public enum DateRangeBetweenOptions
    {
        StartInclusive,
        EndInclusive,
        BothInclusive,
    }
}
