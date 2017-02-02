﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
using System.Globalization;
using ProjectPorcupine.Localization;


#endregion

using System;
using System.Text;
using UnityEngine;

public struct WorldTime : IFormattable
{
    private const int DaysPerQuarter = 15;
    private const int StartingYear = 2999;

    private const float SecondsPerMinute = 60;
    private const float SecondsPerHour = SecondsPerMinute * 60;
    private const float SecondsPerDay = SecondsPerHour * 24;
    private const float SecondsPerQuarter = SecondsPerDay * DaysPerQuarter;
    private const float SecondsPerYear = SecondsPerDay * 4;

    public WorldTime(int seconds)
    {
        Seconds = seconds;
    }

    public WorldTime(int hour, int minute, int second, int day, int quarter, int year)
    {
        if (hour < 0 || hour > 23)
        {
            Debug.LogError("Hour component should be an int from 0-23. Defaulting to 0.");
        }

        if (minute < 0 || minute > 59)
        {
            Debug.LogError("Minute component should be an int from 0-59. Defaulting to 0.");
        }

        if (second < 0 || second > 59)
        {
            Debug.LogError("Second component should be an int from 0-59. Defaulting to 0.");
        }

        if (day < 1 || day > DaysPerQuarter)
        {
            Debug.LogError(string.Format("Day component should be an int from 1-{0}. Defaulting to 1.", DaysPerQuarter));
        }

        if (quarter < 1 || quarter > 4)
        {
            Debug.LogError("Quarter component should be an int from 1-4. Defauling to 1.");
            quarter = 1;
        }

        if (year < 0)
        {
            Debug.LogError(string.Format("Year component should be an int greater than or equal to {0}. Defaulting to {0}", StartingYear));

            year = StartingYear;
        }

        year -= StartingYear;

        int timeComponent = (int)(second + (minute * SecondsPerMinute) + (hour * SecondsPerHour));
        int dateComponent = (int)(((day - 1) * SecondsPerDay) + ((quarter - 1) * SecondsPerQuarter) + (year * SecondsPerYear));
        Seconds = timeComponent + dateComponent;
    }

    public WorldTime(float seconds)
    {
        Seconds = seconds;
    }

    /// <summary>
    /// Gets the seconds since Epoch date (Midnight Q1 Day 1 2999).
    /// </summary>
    /// <value>The seconds since Epoch date (Midnight Q1 Day 1 2999).</value>
    public float Seconds { get; set; }

    /// <summary>
    /// Gets or sets the second component.
    /// </summary>
    /// <value>The second component.</value>
    public int Second
    {
        get
        {
            return (int)(Seconds % 60);
        }

        set
        {
            Seconds -= Second;
            Seconds += value;
        }
    }

    /// <summary>
    /// Gets the minutes since Epoch date (Midnight Q1 Day 1 2999).
    /// </summary>
    /// <value>The minutes since Epoch date (Midnight Q1 Day 1 2999).</value>
    public int Minutes
    {
        get
        {
            return (int)Seconds / 60;
        }
    }

    /// <summary>
    /// Gets or sets the minute component.
    /// </summary>
    /// <value>The minute component.</value>
    public int Minute
    {
        get
        {
            return Minutes % 60;
        }

        set
        {
            Seconds -= Minute * SecondsPerMinute;
            Seconds += value * SecondsPerMinute;
        }
    }

    /// <summary>
    /// Gets the hours since Epoch date (Midnight Q1 Day 1 2999).
    /// </summary>
    /// <value>The hours since Epoch date (Midnight Q1 Day 1 2999).</value>
    public int Hours
    {
        get
        {
            return (int)Minutes / 60;
        }
    }

    /// <summary>
    /// Gets or sets the hour component.
    /// </summary>
    /// <value>The hour component.</value>
    public int Hour
    {
        get
        {
            return Hours % 24;
        }

        set
        {
            Seconds -= Hour * SecondsPerHour;
            Seconds += value * SecondsPerHour;
        }
    }

    /// <summary>
    /// Gets the days since Epoch date (Midnight Q1 Day 1 2999).
    /// </summary>
    /// <value>The days since Epoch date (Midnight Q1 Day 1 2999).</value>
    public int Days
    {
        get
        {
            return (int)Hours / 24;
        }
    }

    /// <summary>
    /// Gets or sets the day component.
    /// </summary>
    /// <value>The day component.</value>
    public int Day
    {
        get
        {
            return (Days % DaysPerQuarter) + 1;
        }

        set
        {
            Seconds -= Day * SecondsPerDay;
            Seconds += value * SecondsPerDay;
        }
    }

    /// <summary>
    /// Gets the quarters since Epoch date (Midnight Q1 Day 1 2999).
    /// </summary>
    /// <value>The quarters since Epoch date (Midnight Q1 Day 1 2999).</value>
    public int Quarters
    {
        get
        {
            return (int)Days / DaysPerQuarter;
        }
    }

    /// <summary>
    /// Gets or sets the quarter component.
    /// </summary>
    /// <value>The quarter component.</value>
    public int Quarter
    {
        get
        {
            return (Quarters % 4) + 1;
        }

        set
        {
            Seconds -= Quarter * SecondsPerQuarter;
            Seconds += value * SecondsPerQuarter;
        }
    }

    /// <summary>
    /// Gets the years since Epoch date (Midnight Q1 Day 1 2999).
    /// </summary>
    /// <value>The years since Epoch date (Midnight Q1 Day 1 2999).</value>
    public int Years
    {
        get
        {
            return (int)Quarters / 4;
        }
    }

    /// <summary>
    /// Gets or sets the year component.
    /// </summary>
    /// <value>The year component.</value>
    public int Year
    {
        get
        {
            return Years + StartingYear;
        }

        set
        {
            Seconds -= Years * SecondsPerYear;
            Seconds += (value - StartingYear) * SecondsPerYear;
        }
    }

    public static WorldTime operator +(WorldTime time1, WorldTime time2)
    {
        WorldTime worldTime = new WorldTime(time1.Seconds + time2.Seconds);
        return worldTime;
    }

    public static WorldTime operator -(WorldTime time1, WorldTime time2)
    {
        WorldTime worldTime = new WorldTime(time1.Seconds - time2.Seconds);
        return worldTime;
    }

    public static bool operator <(WorldTime time1, WorldTime time2)
    {
        return time1.Seconds < time2.Seconds;
    }

    public static bool operator <=(WorldTime time1, WorldTime time2)
    {
        return time1.Seconds <= time2.Seconds;
    }

    public static bool operator >(WorldTime time1, WorldTime time2)
    {
        return time1.Seconds > time2.Seconds;
    }

    public static bool operator >=(WorldTime time1, WorldTime time2)
    {
        return time1.Seconds >= time2.Seconds;
    }

    public static bool operator ==(WorldTime time1, WorldTime time2)
    {
        return Math.Abs(time1.Seconds - time2.Seconds) < float.Epsilon;
    }

    public static bool operator !=(WorldTime time1, WorldTime time2)
    {
        return Math.Abs(time1.Seconds - time2.Seconds) > float.Epsilon;
    }

    #region IFormattable implementation

    string IFormattable.ToString(string format, IFormatProvider provider)
    {
        if (provider == null)
        {
            // This try will always fail with our current language file naming scheme, as it doesn't match up with the standard.
            // This is in place so that should it be changed, we will automatically be using the chosen language rather than the system standard.
            try {
                provider = CultureInfo.GetCultureInfo(LocalizationTable.currentLanguage);
            }
            catch (ArgumentException e)
            {
                provider = CultureInfo.CurrentCulture;
            }
        }

        DateTimeFormatInfo dateTimeFormatInfo = (DateTimeFormatInfo)provider.GetFormat(typeof(DateTimeFormatInfo));

        switch (format)
        {
            case "HH":
                return Hour.ToString("00");
                break;
            case "H":
                return Hour.ToString();
                break;
            case "hh":
                int longhour = Hour;
                if (longhour >= 12)
                {
                    longhour -= 12;
                }

                if (longhour == 0)
                {
                    longhour = 12;
                }

                return longhour.ToString("00");
                break;
            case "h":
                int shorthour = Hour;
                if (shorthour >= 12)
                {
                    shorthour -= 12;
                }

                if (shorthour == 0)
                {
                    shorthour = 12;
                }

                return shorthour.ToString();
                break;
            case "mm":
                return Minute.ToString("00");
                break;
            case "m":
                return Minute.ToString();
                break;
            case "ss":
                return Second.ToString("00");
                break;
            case "s":
                return Second.ToString();
                break;
            case "tt":
                return Hour >= 12 ? dateTimeFormatInfo.PMDesignator : dateTimeFormatInfo.AMDesignator;
            case "q":
                return Quarter.ToString();
            case "d":
                return Day.ToString();
            case "dd":
                return Day.ToString("00");
            case "y":
                return Year.ToString();
            case "G":
                return this.ToString();
            default:
                return string.Empty;
                break;
        }
    }

    #endregion

    /// <summary>
    /// Returns a string that represents the WorldTime time and date (separated by linebreak).
    /// </summary>
    /// <returns>A string that represents the WorldTime time and date.</returns>
    /// <filterpriority>2</filterpriority>
    public override string ToString()
    {
        // TODO: Optimally, we should have WorldTime as an IFormattable, so the format can be more easily adjusted, and doesn't require
        // separate TimeToString and DateToString methods.
        // Note: overloading is used, rather than defaults so that this plays nicely with Lua, which can't see default parameter values properly.
        // return ToString(true, true);
        return string.Format(LocalizationTable.GetLocalization("time_string") + "\n" + LocalizationTable.GetLocalization("date_string"), this);
    }

    /// <summary>
    /// Returns a string that represents the WorldTime, separated by a linebreak if both time and date are present.
    /// </summary>
    /// <returns>The string representing the WorldTime.</returns>
    /// <param name="time">If set to <c>true</c> include the time in the string.</param>
    /// <param name="date">If set to <c>true</c> include the date in the string.</param>
    public string ToString(bool time, bool date)
    {
        StringBuilder sb = new StringBuilder();
        if (time)
        {
            int hour = Hour;
            bool pm = hour >= 12;
            if (pm)
            {
                hour -= 12;
            }

            if (hour == 0)
            {
                hour = 12;
            }

            sb.AppendFormat("{0}:{1:00}{2}", hour, Minute, pm ? "pm" : "am");
        }

        if (time && date)
        {
            sb.AppendLine();
        }

        if (date)
        {
            sb.AppendFormat("Q{0} Day {1}, {2}", Quarter, Day, Year);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a string that represents the WorldTime time.
    /// </summary>
    /// <returns>A string that represents the WorldTime time.</returns>
    public string TimeToString()
    {
        return ToString(true, false);
    }

    /// <summary>
    /// Returns a string that represents the WorldTime date.
    /// </summary>
    /// <returns>A string that represents the WorldTime date.</returns>
    public string DateToString()
    {
        return ToString(false, true);
    }

    /// <summary>
    /// Adds seconds to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="seconds">The seconds to add.</param>
    public WorldTime AddSeconds(int seconds)
    {
        AddSeconds((float)seconds);
        return this;
    }

    /// <summary>
    /// Adds seconds to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="seconds">The seconds to add.</param>
    public WorldTime AddSeconds(float seconds)
    {
        Seconds += seconds;
        return this;
    }

    /// <summary>
    /// Adds minutes to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="minutes">The minutes to add.</param>
    public WorldTime AddMinutes(int minutes)
    {
        Seconds += minutes * SecondsPerMinute;
        return this;
    }

    /// <summary>
    /// Adds hours to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="hours">The hours to add.</param>
    public WorldTime AddHours(int hours)
    {
        Seconds += hours * SecondsPerHour;
        return this;
    }

    /// <summary>
    /// Adds days to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="days">The days to add.</param>
    public WorldTime AddDays(int days)
    {
        Seconds += days * SecondsPerDay;
        return this;
    }

    /// <summary>
    /// Adds quarters to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="quarters">The quarters to add.</param>
    public WorldTime AddQuarters(int quarters)
    {
        Seconds += quarters * SecondsPerQuarter;
        return this;
    }

    /// <summary>
    /// Adds years to the WorldTime object.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="years">The years to add.</param>
    public WorldTime AddYears(int years)
    {
        Seconds += years * SecondsPerYear;
        return this;
    }

    /// <summary>
    /// Sets the second component.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="secondComponent">The second component.</param>
    public WorldTime SetSecond(int secondComponent)
    {
        Second = secondComponent;
        return this;
    }

    /// <summary>
    /// Sets the minute component.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="minuteComponent">The minute component.</param>
    public WorldTime SetMinute(int minuteComponent)
    {
        Minute = minuteComponent;
        return this;
    }

    /// <summary>
    /// Sets the hour component.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="hourComponent">The hour component.</param>
    public WorldTime SetHour(int hourComponent)
    {
        Hour = hourComponent;
        return this;
    }

    /// <summary>
    /// Sets the day component.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="dayComponent">The day component.</param>
    public WorldTime SetDay(int dayComponent)
    {
        Day = dayComponent;
        return this;
    }

    /// <summary>
    /// Sets the quarter component.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="quarterComponent">THe quarter component.</param>
    public WorldTime SetQuarter(int quarterComponent)
    {
        Quarter = quarterComponent;
        return this;
    }

    /// <summary>
    /// Sets the year component.
    /// </summary>
    /// <returns>The WorldTime Object.</returns>
    /// <param name="yearComponent">The year component.</param>
    public WorldTime SetYear(int yearComponent)
    {
        Year = yearComponent;
        return this;
    }
}
