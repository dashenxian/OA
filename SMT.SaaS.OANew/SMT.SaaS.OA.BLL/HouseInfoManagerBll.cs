﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using organClient=SMT.SaaS.BLLCommonServices.OrganizationWS;
using SMT.Foundation.Log;



namespace SMT.SaaS.OA.BLL
{
    public class HouseInfoManagerBll : BaseBll<T_OA_HOUSEINFO>
    {        
        //private HireAppManagementDal hireAppDal = new HireAppManagementDal();
        /// <summary>
        /// 根据ID获取所有可以出租的房源信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_HOUSEINFO> GetHouseInfoById(string houseID)
        {
            try
            {
                var entity = from q in dal.GetTable()
                             where q.HOUSEID == houseID
                             select q;
                return entity.Count() > 0 ? entity.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-GetHouseInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取房源架构树
        /// </summary>
        /// <returns></returns>
        public List<V_HouseInfoTree> GetHouseInfoTree()
        {
            try
            {
                var entity = from p in dal.GetObjects()
                             group p by new
                             {
                                 p.UPTOWN,
                                 p.HOUSENAME,
                                 p.FLOOR
                             }
                                 into g
                                 select new V_HouseInfoTree { UPTOWN = g.Key.UPTOWN, HOUSENAME = g.Key.HOUSENAME, FLOOR = g.Key.FLOOR };
                return entity.Count() > 0 ? entity.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-GetHouseInfoTree" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        //新增
        public bool AddHouse(T_OA_HOUSEINFO houseObj)
        {
            try
            {
                //ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                //        new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);

                if (dal.Add(houseObj) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-AddHouse" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        //修改
        public bool UpdateHouse(T_OA_HOUSEINFO houseObj)
        {
            try
            {
                var entity = from q in dal.GetTable()
                             where q.HOUSEID == houseObj.HOUSEID
                             select q;
                if (entity.Count() > 0)
                {

                    var entitys = entity.FirstOrDefault();
                    CloneEntity(houseObj, entitys); 
                    if (dal.Update(entitys) == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-UpdateHouse" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        //删除
        public bool DeleteHouse(string[] houseID, ref string errorMsg)
        {
            try
            {
                bool result = false;
                var entity = (from ent in dal.GetTable().ToList()
                              where houseID.Contains(ent.HOUSEID) && ent.ISRENT=="0"
                              select ent);
                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        dal.DeleteFromContext(h);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                        result = true;
                }
                else
                {

                    result = false;
                    errorMsg = "此房间此被出租,不能被删除";
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-DeleteHouse" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        //房源记录是否存在
        public bool IsExist(T_OA_HOUSEINFO houseObj)
        {
            try
            {
                var entity = from q in dal.GetTable()
                             where q.UPTOWN == houseObj.UPTOWN && q.HOUSENAME == houseObj.HOUSENAME
                             && q.FLOOR == houseObj.FLOOR && q.ROOMCODE == houseObj.ROOMCODE
                             select q;
                return entity.Count() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-IsExist" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        public IQueryable<V_HouseHirer> HirerQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            //SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();
            try
            {
                string checkState = ((int)CheckStates.Approved).ToString();

                var ents = from q in dal.GetObjects<T_OA_HIREAPP>()
                           join l in dal.GetObjects() on q.T_OA_HOUSELIST.T_OA_HOUSEINFO.HOUSEID equals l.HOUSEID
                           where q.CHECKSTATE == checkState && !q.BACKDATE.HasValue
                           select new V_HouseHirer { houseObj = q, houseInfoObj = l, houseInfo = l.UPTOWN + l.HOUSENAME + l.ROOMCODE };
                ents = ents.Distinct();
                if (paras != null)
                {
                    string uptown = "";
                    string housname = "";
                    Decimal floor = 0;
                    string roomno = "";
                    if (paras.Count == 1)
                    {
                        uptown = paras[0].ToString();
                        ents = ents.Where(s => s.houseInfoObj.UPTOWN == uptown);
                    }
                    else if (paras.Count == 2)
                    {
                        uptown = paras[0].ToString();
                        housname = paras[1].ToString();
                        ents = ents.Where(s => s.houseInfoObj.UPTOWN == uptown && s.houseInfoObj.HOUSENAME == housname);
                    }
                    else if (paras.Count == 3)
                    {
                        uptown = paras[0].ToString();
                        housname = paras[1].ToString();
                        floor = Convert.ToDecimal(paras[2]);
                        if (floor > 0)
                        {
                            //ents = ents.Where(s => s.houseInfoObj.UPTOWN == uptown && s.houseInfoObj.HOUSENAME == housname && s.houseInfoObj.FLOOR == floor);
                        }
                    }
                    else if (paras.Count == 4)
                    {
                        uptown = paras[0].ToString();
                        housname = paras[1].ToString();
                        floor = Convert.ToDecimal(paras[2]);
                        roomno = paras[3].ToString();
                        ents = ents.Where(s => s.houseInfoObj.UPTOWN == uptown && s.houseInfoObj.HOUSENAME == housname && s.houseInfoObj.FLOOR == floor && s.houseInfoObj.ROOMCODE == roomno);
                    }
                }
                //if (!string.IsNullOrEmpty(filterString))
                //{
                //    ents = ents.Where(filterString, paras.ToArray());
                //}
                ents = ents.OrderBy(sort);


                ents = Utility.Pager<V_HouseHirer>(ents, pageIndex, pageSize, ref pageCount);

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-HirerQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
            
        }



        /// <summary>
        /// 获取会议通知和房源发布信息 2010-6-1 by liujx
        /// </summary>
        /// <returns></returns>
        public IQueryable<V_SystemNotice> GetHouseAndNoticeInfo(string userID, List<string> postIDs, List<string> companyIDs, List<string> departmentIDs)
        {
            try
            {
                //增加了发布的范围
                var notice = from p in dal.GetObjects<T_OA_MEETINGMESSAGE>().Include("T_OA_MEETINGINFO")
                             join n in dal.GetObjects<T_OA_MEETINGINFO>() on p.T_OA_MEETINGINFO.MEETINGINFOID equals n.MEETINGINFOID
                             join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on n.MEETINGINFOID equals c.FORMID
                             where n.CHECKSTATE == "2" && (c.VIEWER == userID
                                || postIDs.Contains(c.VIEWER)
                                || departmentIDs.Contains(c.VIEWER)
                                || companyIDs.Contains(c.VIEWER))
                             orderby p.CREATEDATE descending
                             select new V_SystemNotice
                             {
                                 FormId = p.MEETINGMESSAGEID,
                                 FormTitle = p.TITLE,
                                 Formtype = "会议通知",
                                 FormDate = (DateTime)p.UPDATEDATE
                             };
                var house = from k in dal.GetObjects<T_OA_HOUSEINFOISSUANCE>()
                            join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on k.ISSUANCEID equals c.FORMID
                            where k.CHECKSTATE == "2" && (c.VIEWER == userID
                               || postIDs.Contains(c.VIEWER)
                               || departmentIDs.Contains(c.VIEWER)
                               || companyIDs.Contains(c.VIEWER))
                            where k.CHECKSTATE == "2"
                            orderby k.CREATEDATE descending
                            select new V_SystemNotice
                            {
                                FormId = k.ISSUANCEID,
                                FormTitle = k.ISSUANCETITLE,
                                Formtype = "房源发布",
                                FormDate = (DateTime)k.UPDATEDATE
                            };
                //System.Text.Encoding  en = new en
                //19上乱码的问题
                //string str = "神州通";
                //System.Text.Encoding stren = Encoding.GetEncoding("gb2312");
                //byte[] bytes = stren.GetBytes(str);
                ////服务器端按照gb2312正确解码
                //Encoding serverOK = Encoding.GetEncoding("GB2312");
                //string server = serverOK.GetString(bytes);
                string server = "";
                var companydoc = from c in dal.GetObjects<T_OA_SENDDOC>().Include("T_OA_SENDDOCTYPE")
                                 join n in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on c.SENDDOCID equals n.FORMID
                                 where c.CHECKSTATE == "2" && c.ISDISTRIBUTE == "1" &&
                                  (n.VIEWER == userID
                                   || postIDs.Contains(n.VIEWER)
                                  || departmentIDs.Contains(n.VIEWER)
                                  || companyIDs.Contains(n.VIEWER))
                                 orderby c.UPDATEDATE descending
                                 select new V_SystemNotice
                                 {
                                     FormId = c.SENDDOCID,
                                     FormTitle = server + c.NUM.Trim() + "－" + c.SENDDOCTITLE.Trim(),
                                     Formtype = c.T_OA_SENDDOCTYPE.SENDDOCTYPE,
                                     FormDate = (DateTime)c.PUBLISHDATE
                                 };

                var entity = notice.Union(house).Union(companydoc);
                entity = entity.OrderByDescending(c => c.FormDate);


                return entity;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理HouseInfoManagerBll-GetHouseAndNoticeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 获取会议通知和房源发布信息、公文   这个专门供手机版使用 2011-10-19
        /// </summary>
        /// <returns></returns>
        public IQueryable<V_SystemNotice> GetHouseAndNoticeInfoToMobile(int pageIndex, int pageSize,ref int pageCount,ref int DataCount, string userID, List<string> postIDs, List<string> companyIDs, List<string> departmentIDs)
        {
            try
            {
                //增加了发布的范围
                //var notice = from p in dal.GetObjects<T_OA_MEETINGMESSAGE>().Include("T_OA_MEETINGINFO")
                //             join n in dal.GetObjects<T_OA_MEETINGINFO>() on p.T_OA_MEETINGINFO.MEETINGINFOID equals n.MEETINGINFOID
                //             join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on n.MEETINGINFOID equals c.FORMID
                //             where n.CHECKSTATE == "2" && (c.VIEWER == userID
                //                || postIDs.Contains(c.VIEWER)
                //                  || departmentIDs.Contains(c.VIEWER)
                //                  || companyIDs.Contains(c.VIEWER))
                //             orderby p.CREATEDATE descending
                //             select new V_SystemNotice
                //             {
                //                 FormId = p.MEETINGMESSAGEID,
                //                 FormTitle = p.TITLE,
                //                 Formtype = "会议通知",
                //                 FormDate = (DateTime)p.UPDATEDATE
                //             };
                //var house = from k in dal.GetObjects<T_OA_HOUSEINFOISSUANCE>()
                //            join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on k.ISSUANCEID equals c.FORMID
                //            where k.CHECKSTATE == "2" && (c.VIEWER == userID
                //               || postIDs.Contains(c.VIEWER)
                //                  || departmentIDs.Contains(c.VIEWER)
                //                  || companyIDs.Contains(c.VIEWER))
                //            where k.CHECKSTATE == "2"
                //            orderby k.CREATEDATE descending
                //            select new V_SystemNotice
                //            {
                //                FormId = k.ISSUANCEID,
                //                FormTitle = k.ISSUANCETITLE,
                //                Formtype = "房源发布",
                //                FormDate = (DateTime)k.UPDATEDATE
                //            };
                
                var companydoc = from c in dal.GetObjects<T_OA_SENDDOC>().Include("T_OA_SENDDOCTYPE")
                                 join n in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on c.SENDDOCID equals n.FORMID
                                 where c.CHECKSTATE == "2" && c.ISDISTRIBUTE == "1" &&
                                  (n.VIEWER == userID 
                                  || postIDs.Contains( n.VIEWER) 
                                  || departmentIDs.Contains( n.VIEWER) 
                                  || companyIDs.Contains( n.VIEWER))
                                 orderby c.UPDATEDATE descending
                                 select new V_SystemNotice
                                 {
                                     FormId = c.SENDDOCID,
                                     //FormTitle = "神州通" + c.NUM.Trim() + "－" + c.SENDDOCTITLE.Trim(),
                                     FormTitle =  c.NUM.Trim() + "－" + c.SENDDOCTITLE.Trim(),
                                     Formtype = c.T_OA_SENDDOCTYPE.SENDDOCTYPE,
                                     FormDate = (DateTime)c.PUBLISHDATE
                                 };

                //var entity = notice.Union(house).Union(companydoc);
                if (companydoc.Count() >0)
                {
                    companydoc = companydoc.ToList().AsQueryable().OrderByDescending(c => c.FormDate);
                }
                var entity = Utility.PagerMobile<V_SystemNotice>(companydoc, pageIndex, pageSize, ref pageCount, ref DataCount);
                //entity = entity.OrderByDescending(c => c.FormDate);


                return entity;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源管理手机版HouseInfoManagerBll-GetHouseAndNoticeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }




        /// <summary>
        /// 获取会议通知和房源发布信息 2010-6-1 by liujx
        /// </summary>
        /// <returns></returns>
        public T_OA_SENDDOC GetHouseAndNoticeInfoByPriveAndNext(string userID, List<string> postIDs, List<string> companyIDs, List<string> departmentIDs, string formid, ref V_SystemNotice Preview, ref V_SystemNotice Next)
        {
            try
            {
                V_SystemNotice VResult = new V_SystemNotice();
                
                T_OA_SENDDOC EResult = new T_OA_SENDDOC();
                //增加了发布的范围
                //var notice = from p in dal.GetObjects<T_OA_MEETINGMESSAGE>().Include("T_OA_MEETINGINFO")
                //             join n in dal.GetObjects<T_OA_MEETINGINFO>() on p.T_OA_MEETINGINFO.MEETINGINFOID equals n.MEETINGINFOID
                //             join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on n.MEETINGINFOID equals c.FORMID
                //             where n.CHECKSTATE == "2" && (c.VIEWER == userID
                //                || postIDs.Contains(n.VIEWER)
                //                  || departmentIDs.Contains(n.VIEWER)
                //                  || companyIDs.Contains(n.VIEWER))
                //             orderby p.CREATEDATE descending
                //             select new V_SystemNotice
                //             {
                //                 FormId = p.MEETINGMESSAGEID,
                //                 FormTitle = p.TITLE,
                //                 Formtype = "会议通知",
                //                 FormDate = (DateTime)p.UPDATEDATE
                //             };
                //var house = from k in dal.GetObjects<T_OA_HOUSEINFOISSUANCE>()
                //            join c in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on k.ISSUANCEID equals c.FORMID
                //            where k.CHECKSTATE == "2" && (c.VIEWER == userID
                //               || postIDs.Contains(n.VIEWER)
                //                  || departmentIDs.Contains(n.VIEWER)
                //                  || companyIDs.Contains(n.VIEWER))
                //            where k.CHECKSTATE == "2"
                //            orderby k.CREATEDATE descending
                //            select new V_SystemNotice
                //            {
                //                FormId = k.ISSUANCEID,
                //                FormTitle = k.ISSUANCETITLE,
                //                Formtype = "房源发布",
                //                FormDate = (DateTime)k.UPDATEDATE
                //            };

                
                var companydoc = from c in dal.GetObjects<T_OA_SENDDOC>().Include("T_OA_SENDDOCTYPE")
                                 join n in dal.GetObjects<T_OA_DISTRIBUTEUSER>() on c.SENDDOCID equals n.FORMID
                                 where c.CHECKSTATE == "2" && c.ISDISTRIBUTE == "1" &&
                                  (n.VIEWER == userID
                                  || postIDs.Contains(n.VIEWER)
                                  || departmentIDs.Contains(n.VIEWER)
                                  || companyIDs.Contains(n.VIEWER))
                                 orderby c.UPDATEDATE descending
                                 select new V_SystemNotice
                                 {
                                     FormId = c.SENDDOCID,
                                     //FormTitle = "神州通" + c.NUM.Trim() + "－" + c.SENDDOCTITLE.Trim(),
                                     FormTitle = c.NUM.Trim() + "－" + c.SENDDOCTITLE.Trim(),
                                     Formtype = c.T_OA_SENDDOCTYPE.SENDDOCTYPE,
                                     FormDate = (DateTime)c.PUBLISHDATE
                                 };

                //var entity = notice.Union(house).Union(companydoc);
                if (companydoc.Count() > 0)
                {
                    var entity = companydoc.OrderByDescending(c => c.FormDate);
                    if (entity.Count() > 0)
                    {
                        var ents = from ent in entity
                                   where ent.FormId == formid
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            var senddocs = from ent in dal.GetObjects<T_OA_SENDDOC>().Include("T_OA_SENDDOCTYPE")
                                           select ent;
                            if (senddocs != null)
                            {
                                if (ents.Count() > 0)
                                {
                                    VResult = ents.FirstOrDefault();
                                    EResult = senddocs.Where(t => t.SENDDOCID == VResult.FormId).FirstOrDefault();
                                    EResult.CONTENT = null;                                    
                                    int Iindex = 0;
                                    for (int i = 0; i < entity.Count(); i++)
                                    {
                                        if (entity.ToList().ElementAt(i).FormId == formid)
                                        {
                                            Iindex = i;
                                            if (i > 0)
                                            {
                                                Preview = entity.ToList().ElementAt(i - 1);
                                            }
                                            if ((i + 1) < entity.Count())
                                            {
                                                Next = entity.ToList().ElementAt(i + 1);
                                            }
                                            break;
                                        }
                                    }   
                                }
                            }
                        }

                    }
                }
                
                return EResult;
            }
            catch (Exception ex)
            {
                Tracer.Debug("手机获取前后记录管理HouseInfoManagerBll-GetHouseAndNoticeInfoByPriveAndNext" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        
    }


}
