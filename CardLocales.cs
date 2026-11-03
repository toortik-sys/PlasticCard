namespace PlasticCard;

internal static class CardLocales
{
    internal sealed record Text(string Name, string Description);

    private static readonly Dictionary<string, Text> Translations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new("{0} bank card ({1})", "A bank card issued by {0}. Account currency: {1}. Used to store money and pay for purchases."),
        ["ru"] = new("Банковская карта {0} ({1})", "Банковская карта {0}. Валюта счёта: {1}. Используется для хранения денег и оплаты покупок."),
        ["ch"] = new("{0} 银行卡（{1}）", "由 {0} 发行的银行卡。账户币种：{1}。用于存放资金和支付购物款项。"),
        ["cz"] = new("Bankovní karta {0} ({1})", "Bankovní karta vydaná bankou {0}. Měna účtu: {1}. Slouží k ukládání peněz a placení nákupů."),
        ["es"] = new("Tarjeta bancaria {0} ({1})", "Tarjeta bancaria emitida por {0}. Moneda de la cuenta: {1}. Sirve para guardar dinero y pagar compras."),
        ["es-mx"] = new("Tarjeta bancaria {0} ({1})", "Tarjeta bancaria emitida por {0}. Moneda de la cuenta: {1}. Se utiliza para guardar dinero y pagar compras."),
        ["fr"] = new("Carte bancaire {0} ({1})", "Carte bancaire émise par {0}. Devise du compte : {1}. Permet de conserver de l'argent et de régler des achats."),
        ["ge"] = new("{0}-Bankkarte ({1})", "Eine Bankkarte von {0}. Kontowährung: {1}. Zum Aufbewahren von Geld und Bezahlen von Einkäufen."),
        ["hu"] = new("{0} bankkártya ({1})", "A(z) {0} által kibocsátott bankkártya. A számla pénzneme: {1}. Pénz tárolására és vásárlások kifizetésére használható."),
        ["it"] = new("Carta bancaria {0} ({1})", "Carta bancaria emessa da {0}. Valuta del conto: {1}. Consente di conservare denaro e pagare gli acquisti."),
        ["jp"] = new("{0}の銀行カード（{1}）", "{0}が発行した銀行カード。口座通貨：{1}。資金の保管や買い物の支払いに使用する。"),
        ["kr"] = new("{0} 은행 카드 ({1})", "{0}에서 발급한 은행 카드입니다. 계좌 통화: {1}. 돈을 보관하고 물건을 구매할 때 사용합니다."),
        ["pl"] = new("Karta bankowa {0} ({1})", "Karta bankowa wydana przez {0}. Waluta konta: {1}. Służy do przechowywania pieniędzy i płacenia za zakupy."),
        ["po"] = new("Cartão bancário {0} ({1})", "Cartão bancário emitido pelo {0}. Moeda da conta: {1}. Usado para guardar dinheiro e pagar compras."),
        ["ro"] = new("Card bancar {0} ({1})", "Card bancar emis de {0}. Moneda contului: {1}. Folosit pentru păstrarea banilor și plata cumpărăturilor."),
        ["sk"] = new("Banková karta {0} ({1})", "Banková karta vydaná bankou {0}. Mena účtu: {1}. Slúži na uchovávanie peňazí a platenie nákupov."),
        ["tu"] = new("{0} banka kartı ({1})", "{0} tarafından verilen banka kartı. Hesap para birimi: {1}. Para saklamak ve alışverişlerde ödeme yapmak için kullanılır.")
    };

    internal static Text For(string language) => Translations.TryGetValue(language, out var text) ? text : Translations["en"];
}
