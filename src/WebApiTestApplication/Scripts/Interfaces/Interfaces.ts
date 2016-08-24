namespace Interfaces {
    export class DummyClass {
        Name: string;
        Date: DateTime;
        C: AnotherClass;
    
        getQueryParams() {
            return this;
        }
    }

    export class DateTime {
        InternalTicks: number;
        Date: DateTime;
        Day: number;
        DayOfWeek: Enums.DayOfWeek;
        DayOfYear: number;
        Hour: number;
        Kind: Enums.DateTimeKind;
        Millisecond: number;
        Minute: number;
        Month: number;
        Now: DateTime;
        UtcNow: DateTime;
        Second: number;
        Ticks: number;
        TimeOfDay: TimeSpan;
        Today: DateTime;
        Year: number;
    
        getQueryParams() {
            return this;
        }
    }

    export class TimeSpan {
        Ticks: number;
        Days: number;
        Hours: number;
        Milliseconds: number;
        Minutes: number;
        Seconds: number;
        TotalDays: number;
        TotalHours: number;
        TotalMilliseconds: number;
        TotalMinutes: number;
        TotalSeconds: number;
        LegacyMode: boolean;
    
        getQueryParams() {
            return this;
        }
    }

    export class AnotherClass {
        Number: number;
        Name: string;
    
        getQueryParams() {
            return this;
        }
    }
}
