using System.ComponentModel;

namespace JorJika.Helpers.Rows
{
    [DisplayName("ლოგირების ობიექტი")]
    public class ChangeBaseRow
    {
        [DisplayName("UserId")]
        public string UserId { get; set; }

        [DisplayName("User")]
        public string User { get; set; }

        [DisplayName("Identifier")]
        public string Identifier { get; set; }

        [DisplayName("Change Location")]
        public string Location { get; set; }

        [DisplayName("Action")]
        public string Action { get; set; }

        [DisplayName("Field")]
        public string FieldName { get; set; }

        [DisplayName("Old Value")]
        public string OldValue { get; set; }

        [DisplayName("New Value")]
        public string NewValue { get; set; }
    }
}
