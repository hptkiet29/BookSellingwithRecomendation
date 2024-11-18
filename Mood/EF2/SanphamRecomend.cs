namespace Mood.EF2
{
    using Microsoft.ML.Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Sanpham")]
    public partial class SanphamRecomend
    {
        [Key]
        public float IDContent { get; set; }
        
        [Required]
        [StringLength(250)]
        [Display(Name = "Tên sách")]
        public string Name { get; set; }

        [StringLength(250)]
        [Display(Name = "Thẻ SEO")]
        public string MetaTitle { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tác giả")]
        public string TacGia { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nhà xuất bản")]
        public string NhaXuatBan { get; set; }

        

        [Display(Name = "Hình ảnh")]
        public string Images { get; set; }
               
        public float Tophot { get; set; }

        [Display(Name = "Giá Tiền")]
        public float GiaTien { get; set; }
        //[Display(Name = "Giá Khuyến Mại")]
        //public int PriceSale { get; set; }
        [Display(Name = "Lượt Xem")]
        public float LuotXem { get; set; }
    }
    public class ProductPrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
