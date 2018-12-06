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
        private ClaimGasService claimService;
        private UtxoService utxoService;
        private NNSfixedSellingService nnsFixedSellingService;
        private ClaimNNCService claimNNCService;
        private ClaimNNCTransaction claimNNCTransaction;

        private ClaimGasTransaction claimTx4testnet;
        private AuctionRechargeTransaction rechargeTx4testnet;
        private AuctionRechargeTransaction rechargeTx4mainnet;
        
        private httpHelper hh = new httpHelper();
        private mongoHelper mh = new mongoHelper();

        private static Api testApi = new Api("testnet");
        private static Api mainApi = new Api("mainnet");
        public static Api getTestApi() { return testApi; }
        public static Api getMainApi() { return mainApi; }
        private Monitor monitor;

        public Api(string node) {
            netnode = node;
            switch (netnode)
            {
                case "testnet":
                    claimNNCService = new ClaimNNCService
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                    };
                    claimNNCTransaction = new ClaimNNCTransaction
                    {
                        mh = mh,
                        block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        nelJsonRpcUrl = mh.nelJsonRPCUrl_testnet,
                        nncClaimCol = mh.nncClaimCol_testnet,
                        id_gas = mh.id_gas,
                        hash_nnc = mh.hash_nnc,
                        isStartFlag = mh.isStartApplyGasFlag,
                        accountInfo = AccountInfo.getAccountInfoFromWif(mh.prikeywif_testnet),
                    };
                    new Task(() => claimNNCTransaction.claimNNCLoop()).Start();
                    nnsFixedSellingService = new NNSfixedSellingService()
                    {
                        mh = mh,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        NNSfixedSellingColl = mh.NNSfixedSellingColl_testnet,
                        domainCenterColl = mh.domainCenterColl_testnet,
                    };
                    newAuctionService = new NewAuctionService()
                    {
                        mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        auctionStateCol = mh.auctionStateCol_testnet,
                        domainStateCol = mh.domainOwnerCol_testnet,
                        cgasBalanceStateCol = mh.cgasBalanceStateCol_testnet,
                        NNSfixedSellingService = nnsFixedSellingService
                    };
                    AuctionRecharge auctionRechargetTestNet = new AuctionRecharge()
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        nelJsonRPCUrl = mh.neoCliJsonRPCUrl_testnet,
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
                        //Bonus_mongodbConnStr = mh.bonusConnStr_testnet,
                        Bonus_mongodbConnStr = mh.snapshot_mongodbConnStr_testnet,
                        //Bonus_mongodbDatabase = mh.bonusDatabase_testnet,
                        Bonus_mongodbDatabase = mh.snapshot_mongodbDatabase_testnet,
                        CurrentBonusCol = mh.currentBonusCol_testnet,
                        BonusCol = mh.bonusCol_testnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        domainOwnerCol = mh.domainOwnerCol_testnet,
                        NNSfixedSellingService = nnsFixedSellingService
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        //mongodbConnStr_new = mh.analy_mongodbConnStrTestnet,
                        mongodbConnStr_new = mh.analy_mongodbConnStr_testnet,
                        //mongodbDatabase_new = mh.analy_mongodbDatabaseTestnet,
                        mongodbDatabase_new = mh.analy_mongodbDatabase_testnet,
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
                    claimTx4testnet = new ClaimGasTransaction
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
                        checkTxCount = int.Parse(mh.checkTxCount_testnet),
                        isStartFlag = mh.isStartApplyGasFlag,
                    };
                    // 暂时放在这里，后续考虑单独整出来
                    new Task(() => claimTx4testnet.claimGasLoop()).Start();
                    utxoService = new UtxoService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        cgasUtxoCol = mh.cgasUtxoCol_testnet
                    };
                    rechargeTx4testnet = new AuctionRechargeTransaction
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        cgasMergeTxCol = mh.rechargeCollection_testnet,
                        neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_testnet,
                        netType = "testnet",
                        isStartFlag = mh.isStartRechargeFlag,
                    };
                    new Task(() => rechargeTx4testnet.sendTxLoop()).Start();
                    break;
                case "mainnet":
                    nnsFixedSellingService = new NNSfixedSellingService()
                    {
                        mh = mh,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        NNSfixedSellingColl = mh.NNSfixedSellingColl_mainnet,
                        domainCenterColl = mh.domainCenterColl_mainnet,
                    };
                    newAuctionService = new NewAuctionService()
                    {
                        mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        auctionStateCol = mh.auctionStateCol_mainnet,
                        domainStateCol = mh.domainOwnerCol_mainnet,
                        cgasBalanceStateCol = mh.cgasBalanceStateCol_mainnet,
                        NNSfixedSellingService = nnsFixedSellingService
                    };
                    AuctionRecharge auctionRechargetMainNet = new AuctionRecharge()
                    {
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        nelJsonRPCUrl = mh.neoCliJsonRPCUrl_mainnet,
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
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        //Bonus_mongodbConnStr = mh.bonusConnStr_mainnet,
                        Bonus_mongodbConnStr = mh.snapshot_mongodbConnStr_mainnet,
                        //Bonus_mongodbDatabase = mh.bonusDatabase_mainnet,
                        Bonus_mongodbDatabase = mh.snapshot_mongodbDatabase_mainnet,
                        CurrentBonusCol = mh.currentBonusCol_mainnet,
                        BonusCol =mh.bonusCol_mainnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        domainOwnerCol = mh.domainOwnerCol_testnet,
                        NNSfixedSellingService = nnsFixedSellingService
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        //mongodbConnStr_new = mh.analy_mongodbConnStrMainnet,
                        mongodbConnStr_new = mh.analy_mongodbConnStr_mainnet,
                        //mongodbDatabase_new = mh.analy_mongodbDatabaseMainnet
                        mongodbDatabase_new = mh.analy_mongodbDatabase_mainnet
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
                    rechargeTx4mainnet = new AuctionRechargeTransaction
                    {
                        mh = mh,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        cgasMergeTxCol = mh.rechargeCollection_mainnet,
                        neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_mainnet,
                        netType = "mainnet",
                        isStartFlag = mh.isStartRechargeFlag,
                    };
                    new Task(() => rechargeTx4mainnet.sendTxLoop()).Start();
                    break;
            }

            initMonitor();
        }

        public object getRes(JsonRPCrequest req, string reqAddr)
        {
            JArray result = new JArray();
            string resultStr = string.Empty;
            string findFliter = string.Empty;
            string sortStr = string.Empty;
            try
            {
                point(req.method);
                switch (req.method)
                {
                    /**
                     * 新增测试nnc
                     * 1. 申请nnc接口
                     * 2. 查询nnc余额接口
                     */
                    case "hasClaimNNC":
                        result = claimNNCService.hasClaimNNC(req.@params[0].ToString());
                        break;
                    case "claimNNC":
                        result = claimNNCService.claimNNC(req.@params[0].ToString());
                        break;
                    case "getNNCfromSellingHash":
                        result = nnsFixedSellingService.getNNCfromSellingHash(req.@params[0].ToString());
                        break;
                    /***
                     * 新增转让信息和出售信息
                     * 
                     * 1. 首页输入框中显示"出售中"
                     * 2. 首页填写输入框后点击查询详情，显示出售信息：domain + ttl + price
                     * 3. 我的域名管理中新增是否为出售中的状态
                     * 4. 我的域名管理中新增已出售域名列表
                     * 5. 浏览器查询显示初始状态/出售中
                     * 6. 浏览器查询显示域名owner的变化：领取域名 + 转让域名 + 出售域名
                     * 
                     */
                    // 查询已出售列表
                    case "getHasBuyListByAddress":
                        if(req.@params.Length < 4)
                        {
                            result = nnsFixedSellingService.getHasBuyListByAddress(req.@params[0].ToString(), req.@params[1].ToString());
                        } else
                        {
                            result = nnsFixedSellingService.getHasBuyListByAddress(req.@params[0].ToString(), req.@params[1].ToString(), int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()));
                        }
                        break;
                    // 查询出售信息
                    case "getNNSfixedSellingInfo":
                        result = nnsFixedSellingService.getNNSfixedSellingInfo(req.@params[0].ToString());
                        break;
                    // 查询域名竞拍状态+(新增是否正在出售中状态0901)
                    case "getdomainauctioninfo":
                        result = newAuctionService.getdomainAuctionInfo(req.@params[0].ToString());
                        break;
                    // 移动端调用：获取注册器竞拍账户余额
                    case "getregisteraddressbalance":
                        result = newAuctionService.getRegisterAddressBalance(req.@params[0].ToString(), req.@params[1].ToString());
                        break;
                    // 移动端调用：获取竞拍状态
                    case "getauctionstate":
                        result = newAuctionService.getAuctionState(req.@params[0].ToString());
                        break;
                    // 移动端调用：获取域名信息
                    case "getdomaininfo":
                        result = newAuctionService.getDomainInfo(req.@params[0].ToString());
                        break;
                    //
                    case "getresolvedaddress":
                        result = domainService.getResolvedAddress(req.@params[0].ToString());
                        break;
                    case "getavailableutxos":
                        result = utxoService.getAvailableUtxos(req.@params[0].ToString(), Convert.ToDecimal(req.@params[1]));
                        break;
                    case "getCagsLockUtxo":
                        if(req.@params.Length>0)
                            result = utxoService.getCagsLockUtxo(req.@params[0].ToString());
                        else
                            result = utxoService.getCagsLockUtxo();
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
                            result = claimService.claimGas(req.@params[0].ToString(), Convert.ToDecimal(req.@params[1]));
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
                    case "getbonushistbyaddress":
                        if (req.@params.Length < 3)
                        {
                            result = bonusService.getBonusHistByAddress(req.@params[0].ToString());
                        }
                        else
                        {
                            result = bonusService.getBonusHistByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    //申请得到分红
                    case "applybonus":
                        result = bonusService.applyBonus(req.@params[0].ToString());
                        break;
                    //得到最新的分红的信息
                    case "getcurrentbonus":
                        result = bonusService.getCurrentBonus(req.@params[0].ToString());
                        break;
                    //得到某个地址历史分到红的记录
                    case "getbonusbyaddress":
                        if(req.@params.Length < 3)
                            result = bonusService.getBonusByAddress(req.@params[0].ToString());
                        else
                            result = bonusService.getBonusByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    // 根据地址查询域名列表
                    case "getdomainbyaddress":
                        result = domainService.getDomainByAddress(req.@params[0].ToString(), req.@params[1].ToString());
                        break;
                    // 根据地址查询交易列表
                    case "gettransbyaddressOld":
                        result = commonService.getTransByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    case "gettransbyaddress":
                        result = commonService.getTransByAddress_new(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;

                    // test
                    case "getnodetype":
                        result = new JArray { new JObject { { "nodeType", netnode } } };
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

        private void initMonitor()
        {
            string startMonitorFlag = mh.startMonitorFlag;
            if (startMonitorFlag == "1")
            {
                monitor = new Monitor();
            }
        }
        private void point(string method)
        {
            if (monitor != null)
            {
                monitor.point(netnode, method);
            }
        }
    }
}
