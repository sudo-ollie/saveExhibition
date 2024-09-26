using Amazon.DynamoDBv2.DataModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SaveExhibition
{
    [DynamoDBTable("Exhibitions")]
    public class Exhibit
    {
        [DynamoDBHashKey("PK")]
        public string PrivateKey { get; set; }
        [DynamoDBRangeKey("SK")]
        public string SortKey { get; set; }
        public int ExhibitionID { get; set; }
        public bool ExhibitionPublic { get; set; }
        public string ExhibitionName { get; set; }
        public string ExhibitionImage { get; set; }
        public List<ExhibitItem> ExhibitContent { get; set; }
        public int ExhibitionLength { get; set; }
        public string UserID { get; set; }

        public static Exhibit CreateExhibit(
            int exhibitionID,
            bool exhibitionPublic,
            string exhibitionName,
            string exhibitionImage,
            List<ExhibitItem> exhibitContent,
            int exhibitionLength,
            string userID)
        {
            var exhibit = new Exhibit
            {
                ExhibitionID = exhibitionID,
                ExhibitionPublic = exhibitionPublic,
                ExhibitionName = exhibitionName,
                ExhibitionImage = exhibitionImage,
                ExhibitContent = exhibitContent ?? new List<ExhibitItem>(),
                ExhibitionLength = exhibitionLength,
                UserID = userID
            };
            exhibit.CreateKeys();
            return exhibit;
        }

        private void CreateKeys()
        {
            PrivateKey = UserID;
            SortKey = $"{UserID}-{ExhibitionID}";
        }
        public void AddExhibitItem(ExhibitItem newItem)
        {
            ExhibitContent.Add(newItem);
            ExhibitionLength = ExhibitContent.Count;
        }
    }



    public class ExhibitItem
    {
        public string ItemCreditline { get; set; }
        public string ItemDepartment { get; set; }
        public int ItemID { get; set; }
        public string ItemClassification { get; set; }
        public string ItemTechnique { get; set; }
        public string ItemTitle { get; set; }
        public string CreationDate { get; set; }
        public string ArtistBirthplace { get; set; }
        public string ArtistName { get; set; }
        public string ItemObjectLink { get; set; }
        public string ItemCentury { get; set; }
        public string ItemImageURL { get; set; }

        public static ExhibitItem CreateExhibitItem(
            string itemCreditline,
            string itemDepartment,
            int itemID,
            string itemClassification,
            string itemImageURL,
            string artistName,
            string itemTechnique,
            string itemTitle,
            string creationDate,
            string itemObjectLink,
            string itemCentury,
            string artistBirthplace)
        {
            var exhibitItem = new ExhibitItem
            {
                ItemCreditline = itemCreditline,
                ItemDepartment = itemDepartment,
                ItemID = itemID,
                ItemClassification = itemClassification,
                ItemImageURL = itemImageURL,
                ArtistName = artistName,
                ItemTechnique = itemTechnique,
                ItemTitle = itemTitle,
                CreationDate = creationDate,
                ItemObjectLink = itemObjectLink,
                ItemCentury = itemCentury,
                ArtistBirthplace = artistBirthplace
            };
            return exhibitItem;
        }
    }



}