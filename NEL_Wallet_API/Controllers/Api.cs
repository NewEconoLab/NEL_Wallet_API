using NEL_Wallet_API.lib;
using NEL_Wallet_API.RPC;
using NEL_Wallet_API.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace NEL_Wallet_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }

        private NewAuctionService newAuctionService;
        private AuctionService auctionService;
        private BonusService bonusService;
        private DomainService domainService;
        private CommonService commonService;
        private TransactionService txService;
        private ClaimGasService claimService;
        private UtxoService utxoService;

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
                    newAuctionService = new NewAuctionService()
                    {
                        mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        auctionStateCol = mh.auctionStateCol_testnet
                    };
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
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        auctionRecharge = auctionRechargetTestNet,
                    };
                    bonusService = new BonusService
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        BonusNofityCol = mh.bonusNotifyCol_testnet,
                        BonusNofityFrom = mh.bonusNotifyFrom_testnet,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        Bonus_mongodbConnStr = mh.bonusConnStr_testnet,
                        Bonus_mongodbDatabase = mh.bonusDatabase_testnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        domainOwnerCol = mh.domainOwnerCol_testnet,
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        mongodbConnStr = mh.analy_mongodbConnStr_testnet,
                        mongodbDatabase = mh.analy_mongodbDatabase_testnet,
                    };
                    claimService = new ClaimGasService
                    {
                        assetid = mh.id_gas,
                        accountInfo = AccountInfo.getAccountInfoFromWif(mh.prikeywif_testnet),
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        gasClaimCol = mh.gasClaimCol_testnet,
                        maxClaimAmount = int.Parse(mh.maxClaimAmount_testnet),
                    };
                    txService = new TransactionService
                    {
                        nelJsonRpcUrl = mh.nelJsonRPCUrl_testnet,
                        assetid = mh.id_gas,
                        accountInfo = AccountInfo.getAccountInfoFromWif(mh.prikeywif_testnet),
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        gasClaimCol = mh.gasClaimCol_testnet,
                        block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        batchSendInterval = int.Parse(mh.batchSendInterval_testnet),
                        checkTxInterval = int.Parse(mh.checkTxInterval_testnet),
                        checkTxCount = int.Parse(mh.checkTxCount_testnet)
                    };
                    // 暂时放在这里，后续考虑单独整出来
                    new Task(() => txService.claimGasLoop()).Start();
                    utxoService = new UtxoService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        cgasUtxoCol = mh.cgasUtxoCol_testnet
                    };
                    break;
                case "mainnet":
                    newAuctionService = new NewAuctionService()
                    {
                        mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        auctionStateCol = mh.auctionStateCol_mainnet
                    };
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
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        auctionRecharge = auctionRechargetMainNet
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
                        domainOwnerCol = mh.domainOwnerCol_testnet,
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        mongodbConnStr = mh.analy_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.analy_mongodbDatabase_mainnet,
                    };
                    claimService = new ClaimGasService
                    {
                        assetid = mh.id_gas,
                        accountInfo = AccountInfo.getAccountInfoFromWif(mh.prikeywif_mainnet),
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        gasClaimCol = mh.gasClaimCol_mainnet,
                        maxClaimAmount = int.Parse(mh.maxClaimAmount_mainnet),
                    };
                    utxoService = new UtxoService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        cgasUtxoCol = mh.cgasUtxoCol_mainnet
                    };
                    break;
            }
        }

        public object getRes(JsonRPCrequest req, string reqAddr)
        {
            JArray result = new JArray();
            string resultStr = string.Empty;
            string findFliter = string.Empty;
            string sortStr = string.Empty;
            try
            {
                switch (req.method)
                {
                    case "getresolvedaddress":
                        result = domainService.getResolvedAddress(req.@params[0].ToString());
                        break;
                    case "getavailableutxos":
                        result = utxoService.getAvailableUtxos(req.@params[0].ToString(), decimal.Parse(req.@params[1].ToString()));
                        break;
                    case "getauctioninfocount":
                        if (req.@params.Length < 2)
                        {
                            result = newAuctionService.getAcutionInfoCount(req.@params[0].ToString());
                        } else
                        {
                            result = newAuctionService.getAcutionInfoCount(req.@params[0].ToString(), req.@params[1].ToString());
                        }
                        break;
                    case "getauctioninfobyaddress":
                        if (req.@params.Length < 3)
                        {
                            result = newAuctionService.getAuctionInfoByAddress(req.@params[0].ToString());
                        }
                        else if(req.@params.Length < 4)
                        {
                            result = newAuctionService.getAuctionInfoByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        } else
                        {
                            result = newAuctionService.getAuctionInfoByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), req.@params[3].ToString());
                        }
                        break;
                    case "getauctioninfobyaucitonid":
                        string address = req.@params[0].ToString();  // address
                        string auctionIdsStr = req.@params[1].ToString();             // auctionIdArr
                        result = newAuctionService.getAuctionInfoByAuctionId(JArray.Parse(auctionIdsStr), address);
                        break;
                    // 查询是否可以申领Gas
                    case "hasclaimgas":
                        result = claimService.hasClaimGas(req.@params[0].ToString());
                        break;
                    // 申领Gas(即向客户地址转账，默认1gas
                    case "claimgas":
                        if (req.@params.Length < 2)
                        {
                            result = claimService.claimGas(req.@params[0].ToString());
                        }
                        else
                        {
                            result = claimService.claimGas(req.@params[0].ToString(), decimal.Parse(req.@params[1].ToString()));
                        }
                        break;
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
                    case "getbonushistbyaddressOld":
                        if(req.@params.Length < 3)
                        {
                            result = bonusService.getBonusHistByAddress(req.@params[0].ToString());
                        } else
                        {
                            result = bonusService.getBonusHistByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "getbonushistbyaddress":
                        if (req.@params.Length < 3)
                        {
                            result = bonusService.getBonusHistByAddressNew(req.@params[0].ToString());
                        }
                        else
                        {
                            result = bonusService.getBonusHistByAddressNew(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    // 根据地址查询域名
                    case "getdomainbyaddress":
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
