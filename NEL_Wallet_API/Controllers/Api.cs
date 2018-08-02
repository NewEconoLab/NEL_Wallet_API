using NEL_Wallet_API.lib;
using NEL_Wallet_API.RPC;
using NEL_Wallet_API.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace NEL_Wallet_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }
        
        private AuctionService auctionService;
        private BonusService bonusService;
        private DomainService domainService;
        private CommonService commonService;

        private httpHelper hh = new httpHelper();
        private mongoHelper mh = new mongoHelper();

        private static Api testApi = new Api("testnet");
        private static Api mainApi = new Api("mainnet");
        public static Api getTestApi() { return testApi; }
        public static Api getMainApi() { return mainApi; }

        public Api(string node) {
            netnode = node;
            switch (netnode)
            {
                case "testnet":
                    AuctionRecharge auctionRechargetTestNet = new AuctionRecharge()
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        nelJsonRPCUrl = mh.nelJsonRPCUrl_testnet,
                        rechargeCollection = mh.rechargeCollection_testnet
                    };
                    auctionService = new AuctionService()
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        queryDomainCollection = mh.queryDomainCollection_testnet,
                        queryBidListCollection = mh.queryBidListCollection_testnet,
                        auctionRecharge = auctionRechargetTestNet,
                        domainStateCol = mh.domainStateCol_testnet,
                        domainUserStateCol = mh.domainUserStateCol_testnet
                    };
                    bonusService = new BonusService
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        BonusNofityCol = mh.bonusNotifyCol_testnet,
                        BonusNofityFrom = mh.bonusNotifyFrom_testnet,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        queryDomainCollection = mh.queryDomainCollection_testnet,
                        domainResolver = mh.domainResolver_testnet,
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        mongodbConnStr = mh.analy_mongodbConnStr_testnet,
                        mongodbDatabase = mh.analy_mongodbDatabase_testnet,
                    };


                    break;
                case "mainnet":
                    AuctionRecharge auctionRechargetMainNet = new AuctionRecharge()
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        nelJsonRPCUrl = mh.nelJsonRPCUrl_mainnet,
                        rechargeCollection = mh.rechargeCollection_mainnet
                    };
                    auctionService = new AuctionService()
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        queryDomainCollection = mh.queryDomainCollection_mainnet,
                        queryBidListCollection = mh.queryBidListCollection_mainnet,
                        auctionRecharge = auctionRechargetMainNet,
                        domainStateCol = mh.domainStateCol_mainnet,
                        domainUserStateCol = mh.domainUserStateCol_mainnet

                    };
                    bonusService = new BonusService
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        BonusNofityCol = mh.bonusNotifyCol_mainnet,
                        BonusNofityFrom = mh.bonusNotifyFrom_mainnet,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        queryDomainCollection = mh.queryDomainCollection_mainnet,
                        domainResolver = mh.domainResolver_mainnet,
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        mongodbConnStr = mh.analy_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.analy_mongodbDatabase_mainnet,
                    };
                    break;
            }
        }

        public object getRes(JsonRPCrequest req,string reqAddr)
        {
            JArray result = new JArray();
            string resultStr = string.Empty;
            string findFliter = string.Empty;
            string sortStr = string.Empty;
            try
            {
                switch (req.method)
                {
                    // 根据txid查询交易是否成功
                    case "hastx":
                        result = auctionService.hasTx(req.@params[0].ToString());
                        break;
                    // 根据txid查询合约是否成功
                    case "hascontract":
                        result = auctionService.hasContract(req.@params[0].ToString());
                        break;
                    // 充值&转账
                    case "rechargeandtransfer":
                        result = auctionService.rechargeAndTransfer(req.@params[0].ToString(), req.@params[1].ToString());
                        break;
                    // 查询充值&转账交易
                    case "getrechargeandtransfer":
                        result = auctionService.getRechargeAndTransfer(req.@params[0].ToString());
                        break;
                    // 根据地址查询分红历史
                    case "getbonushistbyaddress":
                        if(req.@params.Length < 3)
                        {
                            result = bonusService.getBonusHistByAddress(req.@params[0].ToString());
                        } else
                        {
                            result = bonusService.getBonusHistByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    // 根据地址查询竞拍域名列表(域名支持模糊匹配)
                    case "searchdomainbyaddressNew":
                        if (req.@params.Length < 3)
                        {
                            result = auctionService.getBidListByAddressLikeDomainNew(req.@params[0].ToString(), req.@params[1].ToString());
                        }
                        else
                        {
                            result = auctionService.getBidListByAddressLikeDomainNew(req.@params[0].ToString(), req.@params[1].ToString(), int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()));
                        }
                        break;
                    case "searchdomainbyaddress":
                        if (req.@params.Length < 3)
                        {
                            result = auctionService.getBidListByAddressLikeDomain(req.@params[0].ToString(), req.@params[1].ToString());
                        } else
                        {
                            result = auctionService.getBidListByAddressLikeDomain(req.@params[0].ToString(), req.@params[1].ToString(), int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()));
                        }
                        break;
                    // 查询域名状态
                    case "getdomainstate":
                        result = auctionService.getDomainState(req.@params[0].ToString(), req.@params[1].ToString());
                        break;
                    // 根据地址查询竞拍域名列表
                    case "getbidlistbyaddress":
                        if (req.@params.Length < 3)
                        {
                            result = auctionService.getBidListByAddressNew(req.@params[0].ToString());
                        } else
                        {
                            result = auctionService.getBidListByAddressNew(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "getbidlistbyaddressOld":
                        if(req.@params.Length < 3)
                        {
                            result = auctionService.getBidListByAddress(req.@params[0].ToString());
                        } else
                        {
                            result = auctionService.getBidListByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    // 根据域名查询域名竞拍详情
                    case "getbiddetailbydomain":
                        if (req.@params.Length < 3)
                        {
                            result = auctionService.getBidDetailByAuctionId(req.@params[0].ToString());
                        }
                        else
                        {
                            result = auctionService.getBidDetailByAuctionId(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "getbiddetailbydomainOld":
                        if (req.@params.Length < 3)
                        { 
                            result = auctionService.getBidDetailByDomain(req.@params[0].ToString());
                        } else
                        {
                            result = auctionService.getBidDetailByDomain(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    // 根据域名查询域名竞拍结果
                    case "getbidresbydomain":
                        result = auctionService.getBidResByDomain(req.@params[0].ToString());
                        break;
                    // 根据地址查询域名
                    case "getdomainbyaddress":
                        result = domainService.getDomainByAddressNew(req.@params[0].ToString(), req.@params[1].ToString());
                        break;
                    case "getdomainbyaddressOld":
                        result = domainService.getDomainByAddress(req.@params[0].ToString(), req.@params[1].ToString());
                        break;
                    // 根据地址查询交易列表
                    case "gettransbyaddress":
                        result = commonService.getTransByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                }
                if (result.Count == 0)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -1, "No Data", "Data does not exist");
                    return resE;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("errMsg:{0},errStack:{1}", e.Message, e.StackTrace);
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -100, "Parameter Error", e.Message);
                return resE;
            }

            JsonPRCresponse res = new JsonPRCresponse();
            res.jsonrpc = req.jsonrpc;
            res.id = req.id;
            res.result = result;

            return res;
        }
    }
}
