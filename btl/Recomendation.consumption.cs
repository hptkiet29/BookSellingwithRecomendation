﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
namespace Dtbso
{
    public partial class Recomendation
    {
        /// <summary>
        /// model input class for Recomendation.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"IDContent")]
            public float IDContent { get; set; }

            [ColumnName(@"Name")]
            public string Name { get; set; }

            [ColumnName(@"MetaTitle")]
            public string MetaTitle { get; set; }

            [ColumnName(@"TacGia")]
            public string TacGia { get; set; }

            [ColumnName(@"NhaXuatBan")]
            public string NhaXuatBan { get; set; }

            [ColumnName(@"Soluong")]
            public float Soluong { get; set; }

            [ColumnName(@"Images")]
            public string Images { get; set; }

            [ColumnName(@"CategoryID")]
            public float CategoryID { get; set; }

            [ColumnName(@"Quanlity")]
            public string Quanlity { get; set; }

            [ColumnName(@"NgayTao")]
            public DateTime NgayTao { get; set; }

            [ColumnName(@"IDNguoiTao")]
            public float IDNguoiTao { get; set; }

            [ColumnName(@"Status")]
            public bool Status { get; set; }

            [ColumnName(@"Tophot")]
            public float Tophot { get; set; }

            [ColumnName(@"Mota")]
            public string Mota { get; set; }

            [ColumnName(@"ChiTiet")]
            public string ChiTiet { get; set; }

            [ColumnName(@"IDNCC")]
            public float IDNCC { get; set; }

            [ColumnName(@"GiaTien")]
            public float GiaTien { get; set; }

            [ColumnName(@"GiaNhap")]
            public float GiaNhap { get; set; }

            [ColumnName(@"PriceSale")]
            public float PriceSale { get; set; }

            [ColumnName(@"TonKho")]
            public float TonKho { get; set; }

            [ColumnName(@"LuotXem")]
            public float LuotXem { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for Recomendation.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"IDContent")]
            public float IDContent { get; set; }

            [ColumnName(@"Name")]
            public float[] Name { get; set; }

            [ColumnName(@"MetaTitle")]
            public string MetaTitle { get; set; }

            [ColumnName(@"TacGia")]
            public float[] TacGia { get; set; }

            [ColumnName(@"NhaXuatBan")]
            public float[] NhaXuatBan { get; set; }

            [ColumnName(@"Soluong")]
            public float Soluong { get; set; }

            [ColumnName(@"Images")]
            public string Images { get; set; }

            [ColumnName(@"CategoryID")]
            public float CategoryID { get; set; }

            [ColumnName(@"Quanlity")]
            public string Quanlity { get; set; }

            [ColumnName(@"NgayTao")]
            public DateTime NgayTao { get; set; }

            [ColumnName(@"IDNguoiTao")]
            public float IDNguoiTao { get; set; }

            [ColumnName(@"Status")]
            public bool Status { get; set; }

            [ColumnName(@"Tophot")]
            public float Tophot { get; set; }

            [ColumnName(@"Mota")]
            public string Mota { get; set; }

            [ColumnName(@"ChiTiet")]
            public string ChiTiet { get; set; }

            [ColumnName(@"IDNCC")]
            public float IDNCC { get; set; }

            [ColumnName(@"GiaTien")]
            public float GiaTien { get; set; }

            [ColumnName(@"GiaNhap")]
            public float GiaNhap { get; set; }

            [ColumnName(@"PriceSale")]
            public float PriceSale { get; set; }

            [ColumnName(@"TonKho")]
            public float TonKho { get; set; }

            [ColumnName(@"LuotXem")]
            public float LuotXem { get; set; }

            [ColumnName(@"Features")]
            public float[] Features { get; set; }

            [ColumnName(@"Score")]
            public float Score { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Path.GetFullPath("Recomendation.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}