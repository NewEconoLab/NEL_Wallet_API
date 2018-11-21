﻿using NEL_Wallet_API.lib;
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

        private ClaimGasTransaction claimTx4testnet;
        private AuctionRechargeTransaction rechargeTx4testnet;
        private AuctionRechargeTransaction rechargeTx4mainnet;
        
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
                    nnsFixedSellingService = new NNSfixedSellingService()
                    {
                        mh = mh,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
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
                        Bonus_mongodbConnStr = mh.bonusConnStr_testnet,
                        Bonus_mongodbDatabase = mh.bonusDatabase_testnet
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
                        mongodbConnStr_new = mh.analy_mongodbConnStrTestnet,
                        mongodbDatabase_new = mh.analy_mongodbDatabaseTestnet
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
                        checkTxCount = int.Parse(mh.checkTxCount_testnet)
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
                        netType = "testnet"
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
                        mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        mongodbConnStr_new = mh.analy_mongodbConnStrMainnet,
                        mongodbDatabase_new = mh.analy_mongodbDatabaseMainnet
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
                        netType = "mainnet"
                    };
                    new Task(() => rechargeTx4mainnet.sendTxLoop()).Start();
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
