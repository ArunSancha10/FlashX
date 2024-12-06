namespace outofoffice.Models
{
    public class SharePointModel
    {
        public string Id { get; set; }
        public string Jan { get; set; }
        public string Feb { get; set; }
        public string Mar { get; set; }
        public string Apr { get; set; }
        public string May { get; set; }
        public string Jun { get; set; }
        public string Jul { get; set; }
        public string Aug { get; set; }
        public string Sep { get; set; }
        public string Oct { get; set; }
        public string Nov { get; set; }
        public string Dec { get; set; }

        public void MapFromDictionary(IDictionary<string, object> additionalData)
        {
            foreach (var kvp in additionalData)
            {
                var property = this.GetType().GetProperty(kvp.Key);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(this, Convert.ChangeType(kvp.Value, property.PropertyType));
                }
            }
        }


        public Dictionary<string, object> ToDictionary()
        {
            var additionalData = new Dictionary<string, object>();

            foreach (var property in this.GetType().GetProperties())
            {
                var value = property.GetValue(this);

                if (property.Name.Equals("Id")) continue;

                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    additionalData[property.Name] = value;
                }
            }

            return additionalData;
        }


    }
}
