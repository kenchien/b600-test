namespace TldcFare.WebApi.Models {
   public class SevViewModel {
      public string RequestNum { get; set; }
      public string SevId { get; set; }
      public string SevName { get; set; }
      public string SevIdno { get; set; }
      public string Status { get; set; }
      public string GrpId { get; set; }
      public string BranchId { get; set; }
      public string JobTitle { get; set; }
      public string PreSevId { get; set; }

      public string JoinDate { get; set; }
      public string ExceptDate { get; set; }
      public string Birthday { get; set; }
      public int Age { get; set; }
      public string SexType { get; set; }
      public string ContName { get; set; }
      public string Mobile { get; set; }
      public string Mobile2 { get; set; }
      public string Phone { get; set; }
      public string Email { get; set; }
      public string RegZipCode { get; set; }
      public string RegAddress { get; set; }
      public string ZipCode { get; set; }
      public string Address { get; set; }

      public string NoticeName { get; set; }
      public string NoticeRelation { get; set; }
      public string NoticeZipCode { get; set; }
      public string NoticeAddress { get; set; }
      public string PayeeName { get; set; }
      public string PayeeIdno { get; set; }
      public string PayeeRelation { get; set; }
      public string PayeeBirthday { get; set; }
      public string PayeeBank { get; set; }
      public string PayeeBranch { get; set; }
      public string PayeeAcc { get; set; }
      public string Remark { get; set; }
      public string SendDate { get; set; }
      public string Sender { get; set; }
      public string ReviewDate { get; set; }
      public string Reviewer { get; set; }
      public string CreateDate { get; set; }
      public string CreateUser { get; set; }
      public string UpdateDate { get; set; }
      public string UpdateUser { get; set; }

      public string InitialSevId { get; set; }

      /// <summary>
      /// 升組長日
      /// </summary>
      public string PromoteDate2 { get; set; }

      /// <summary>
      /// 升處長日
      /// </summary>
      public string PromoteDate3 { get; set; }

      /// <summary>
      /// 升督導日
      /// </summary>
      public string PromoteDate4 { get; set; }

      /// <summary>
      /// 組長回訓日
      /// </summary>
      public string RetrainDate2 { get; set; }

      /// <summary>
      /// 處長回訓日
      /// </summary>
      public string RetrainDate3 { get; set; }

      /// <summary>
      /// 督導回訓日
      /// </summary>
      public string RetrainDate4 { get; set; }
      public string FirstClassDate { get; set; }//初階課程日
      public string SecondClassDate { get; set; }//中階課程日
      public string ThirdClassDate { get; set; }//高階課程日
   }

   public class CreateSevModel {
      public SevViewModel NewSevInfo { get; set; }
      public string CreateUser { get; set; }
   }

   public class UpdateSevModel {
      public SevViewModel SevInfo { get; set; }
      public string UpdateUser { get; set; }
   }
}