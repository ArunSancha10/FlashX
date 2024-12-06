using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using TimeZoneConverter;

namespace outofoffice.TagHelpers
{
    public class DateTimeOffsetTagHelper : TagHelper
    {

        public DateTimeOffset? Value { get; set; }
        public string TimeZone { get; set; }
        public string Format { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        public bool IsEditable { get; set; }

        public string DateFormat { get; set; } = "yyyy-MM-dd"; // Default HTML5 date input format
        public string TimeFormat { get; set; } = "hh:mm tt";  // Default 12-hour time format


        //public override void Process(TagHelperContext context, TagHelperOutput output)
        //{
        //    if (string.IsNullOrWhiteSpace(Format))
        //    {
        //        // Handle empty format
        //        // Default to
        //        Format = "g";
        //    }

        //    DateTime dateTime;
        //    try
        //    {
        //        // TZConvert handles either an IANA or Windows identifier
        //        // Graph can return either
        //        var userTimeZone = TZConvert.GetTimeZoneInfo(TimeZone);
        //        dateTime = TimeZoneInfo.ConvertTimeFromUtc(Value.GetValueOrDefault().UtcDateTime, userTimeZone);
        //    }
        //    catch (TimeZoneNotFoundException)
        //    {
        //        // If the time zone isn't found, just use
        //        dateTime = Value.GetValueOrDefault().UtcDateTime;
        //    }

        //    if (IsEditable)
        //    {
        //        // Render as input
        //        output.TagName = "input";
        //        output.Attributes.SetAttribute("type", "datetime-local");
        //        output.Attributes.SetAttribute("value", dateTime.ToString(Format));
        //        output.Attributes.SetAttribute("data-timezone", TimeZone); // Optional metadata
        //        output.Attributes.SetAttribute("class", "form-control"); // Optional styling
        //    }
        //    else
        //    {
        //        // Render as span
        //        output.TagName = "span";
        //        output.Content.SetContent(dateTime.ToString(Format));
        //    }
        //}

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Value == null)
            {
                Value = DateTimeOffset.UtcNow; // Default to current time if value is null
            }

            DateTime dateTime;
            try
            {
                var userTimeZone = TZConvert.GetTimeZoneInfo(TimeZone);
                dateTime = TimeZoneInfo.ConvertTimeFromUtc(Value.GetValueOrDefault().UtcDateTime, userTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                dateTime = Value.GetValueOrDefault().UtcDateTime;
            }

            // Extract date and time
            var date = dateTime.ToString(DateFormat);
            var time = dateTime.ToString(TimeFormat);

            // Render as editable input fields or readonly spans
            if (IsEditable)
            {
                output.TagName = null;

                output.Content.SetHtmlContent($@"
                    <div class='row g-3 align-items-center'>
                        <div class='col-6'>
                            <input id='{Id}-date' name='{Name}_Date' type='date' value='{date}' class='form-control' data-type='date' />
                        </div>
                        <div class='col-6'>
                            <input id='{Id}-time' name='{Name}_Time' type='time' value='{dateTime:HH:mm}' class='form-control' data-type='time' />
                        </div>
                    </div>
                ");

            }
            else
            {
                // Render two spans
                output.TagName = null; // Remove the wrapping element

                output.Content.SetHtmlContent($@"
    <div class=""col-4"">
        <label for=""startDate"">Start time</label>
    </div>
    <div class=""col-4"">
        <div class=""input-container"">
            <input type=""date"" class=""time-input"" id=""startDate"" value=""{date:yyyy-MM-dd}"">
        </div>
    </div>
    <div class=""col-4"">
        <div class=""input-container"">
            <input type=""time"" class=""time-input"" id=""startTime"" value=""{dateTime:HH:mm}"">
        </div>
    </div>
");

            }
        }

    }
}
