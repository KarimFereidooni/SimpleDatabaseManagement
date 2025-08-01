using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
static class PC
{
    static System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
    //--------------------------------------------
    public static string GetDate(DateTime dateTime, string separator)
    {
        if (separator == "")
            separator = "/";
        string s;
        if (pc.GetYear(dateTime).ToString().Length == 1)
            s = "0" + pc.GetYear(dateTime).ToString();
        else
            s = pc.GetYear(dateTime).ToString();
        s += separator;
        if (pc.GetMonth(dateTime).ToString().Length == 1)
            s += "0" + pc.GetMonth(dateTime).ToString();
        else
            s += pc.GetMonth(dateTime).ToString();
        s += separator;
        if (pc.GetDayOfMonth(dateTime).ToString().Length == 1)
            s += "0" + pc.GetDayOfMonth(dateTime).ToString();
        else
            s += pc.GetDayOfMonth(dateTime).ToString();
        return s;
    }
    public static string GetDate(DateTime dateTime)
    {
        string separator = "/";
        string s;
        if (pc.GetYear(dateTime).ToString().Length == 1)
            s = "0" + pc.GetYear(dateTime).ToString();
        else
            s = pc.GetYear(dateTime).ToString();
        s += separator;
        if (pc.GetMonth(dateTime).ToString().Length == 1)
            s += "0" + pc.GetMonth(dateTime).ToString();
        else
            s += pc.GetMonth(dateTime).ToString();
        s += separator;
        if (pc.GetDayOfMonth(dateTime).ToString().Length == 1)
            s += "0" + pc.GetDayOfMonth(dateTime).ToString();
        else
            s += pc.GetDayOfMonth(dateTime).ToString();
        return s;
    }
    public static string GetDate(DateTime dateTime, bool insert_zero)
    {
        string separator = "/";
        string s;
        if (pc.GetYear(dateTime).ToString().Length == 1 && insert_zero == true)
            s = "0" + pc.GetYear(dateTime).ToString();
        else
            s = pc.GetYear(dateTime).ToString();
        s += separator;
        if (pc.GetMonth(dateTime).ToString().Length == 1 && insert_zero == true)
            s += "0" + pc.GetMonth(dateTime).ToString();
        else
            s += pc.GetMonth(dateTime).ToString();
        s += separator;
        if (pc.GetDayOfMonth(dateTime).ToString().Length == 1 && insert_zero == true)
            s += "0" + pc.GetDayOfMonth(dateTime).ToString();
        else
            s += pc.GetDayOfMonth(dateTime).ToString();
        return s;
    }
    //--------------------------------------------
    public static string GetDateWithMonthName(DateTime dateTime, string separator)
    {
        if (separator == "")
            separator = "/";
        string s;
        s = pc.GetDayOfMonth(dateTime).ToString();
        s += separator;
        s += GetMonthName(dateTime);
        s += separator;
        s += pc.GetYear(dateTime).ToString();
        return s;
    }
    public static string GetDateWithMonthName(DateTime dateTime)
    {
        string s;
        s = pc.GetDayOfMonth(dateTime).ToString();
        s += GetMonthName(dateTime);
        s += pc.GetYear(dateTime).ToString();
        return s;
    }
    //--------------------------------------------
    public static string GetTime()
    {
        string h = DateTime.Now.Hour.ToString(), m = DateTime.Now.Minute.ToString();
        if (h.Length == 1)
            h = "0" + h;
        if (m.Length == 1)
            m = "0" + m;
        return h + ":" + m;
    }
    public static string GetTime(string separator)
    {
        string h = DateTime.Now.Hour.ToString(), m = DateTime.Now.Minute.ToString();
        if (h.Length == 1)
            h = "0" + h;
        if (m.Length == 1)
            m = "0" + m;
        return h + separator + m;
    }
    public static string GetTime(bool insertZero, bool insertSecond)
    {
        string h = DateTime.Now.Hour.ToString(), m = DateTime.Now.Minute.ToString(), s = DateTime.Now.Second.ToString();
        if (h.Length == 1 && insertZero == true)
            h = "0" + h;
        if (m.Length == 1 && insertZero == true)
            m = "0" + m;
        if (s.Length == 1 && insertZero == true)
            s = "0" + s;
        if (insertSecond)
            return h + ":" + m + ":" + s;
        else
            return h + ":" + m;
    }
    //--------------------------------------------
    public static string GetMonthName(DateTime dateTime)
    {
        switch (pc.GetMonth(dateTime))
        {
            case 1:
                return "فروردین";
            case 2:
                return "اردیبهشت";
            case 3:
                return "خرداد";
            case 4:
                return "تیر";
            case 5:
                return "مرداد";
            case 6:
                return "شهریور";
            case 7:
                return "مهر";
            case 8:
                return "آبان";
            case 9:
                return "آذر";
            case 10:
                return "دی";
            case 11:
                return "بهمن";
            case 12:
                return "اسفند";
            default:
                return "";
        }
    }
    //--------------------------------------------
    public static int GetDayOfWeekCode(DateTime dateTime)
    {
        switch (pc.GetDayOfWeek(dateTime))
        {
            case DayOfWeek.Friday:
                return 7;
            case DayOfWeek.Monday:
                return 3;
            case DayOfWeek.Saturday:
                return 1;
            case DayOfWeek.Sunday:
                return 2;
            case DayOfWeek.Thursday:
                return 6;
            case DayOfWeek.Tuesday:
                return 4;
            case DayOfWeek.Wednesday:
                return 5;

        }
        return 0;
    }
    public static int GetDayOfWeekCode(DateTime dateTime, int day)
    {
        DateTime t = pc.ToDateTime(pc.GetYear(dateTime), pc.GetMonth(dateTime), day, 12, 0, 0, 0);
        return GetDayOfWeekCode(t);
    }
    //--------------------------------------------
    public static string GetDayOfWeek(DateTime dateTime)
    {
        switch (pc.GetDayOfWeek(dateTime))
        {
            case DayOfWeek.Friday:
                return "جمعه";
            case DayOfWeek.Monday:
                return "دو شنبه";
            case DayOfWeek.Saturday:
                return "شنبه";
            case DayOfWeek.Sunday:
                return "يك شنبه";
            case DayOfWeek.Thursday:
                return "پنج شنبه";
            case DayOfWeek.Tuesday:
                return "سه شنبه";
            case DayOfWeek.Wednesday:
                return "چهار شنبه";

        }
        return "";
    }
    //--------------------------------------------
    public static int GetNumberOfDays(DateTime dateTime)
    {
        return pc.GetDaysInMonth(GetYear(dateTime), GetMonth(dateTime));
    }
    //--------------------------------------------
    public static int GetYear(DateTime dateTime)
    {
        return pc.GetYear(dateTime);
    }
    public static int GetMonth(DateTime dateTime)
    {
        return pc.GetMonth(dateTime);
    }
    public static int GetDay(DateTime dateTime)
    {
        return pc.GetDayOfMonth(dateTime);
    }
    //--------------------------------------------

}

