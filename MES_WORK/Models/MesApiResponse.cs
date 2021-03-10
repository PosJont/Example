using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES_WORK.Models
{
    public class MesApiResponse
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public void Set_MesResponse(string status, string message)
        {
            Result = status;
            Message = message;
        }
    }
    public class SetBasic_Data
    {
        public string sUser { get; set; }
        public string sMoCode { get; set; }
        public string sMacCode { get; set; }
        public string sWrkCode { get; set; }
        public string sStationCode { get; set; }
        public string sProCode { get; set; }
        public string sProQty { get; set; }
        public string sLotNo { get; set; }
        public string sWorkCode { get; set; }
        public string sPalletCode { get; set; }
        public string sToken { get; set; }
        public string sNgCode { get; set; }
        }



    public class MacData
    {
        public string pToken { get; set; }
        public string pMacCode { get; set; }
        public string pDate { get; set; }
    }
    public class JsonData
    {
        public string pToken { get; set; }
        public string pJson { get; set; }
    }
}