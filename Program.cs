using System.Runtime.CompilerServices;

namespace MiniSistemaPortifolioDeInvestimento
{

    //Define o contrato que deve ser implementado por qualquer classe que modele
    //um ativo financeiro dentro do sistema.

    // S - Single Responsibility: cada interface define uma responsabilidade única
    public interface IAtivoFinanceiro
    {
            string Nome { get; }
            int Quantidade { get; }       //acresentei porque todo ativo tem quantidade
            decimal ValorInvestido { get; }
            decimal ValorAtual { get; }
            decimal CalcularRentabilidade(); //Calcula a rentabilidade do ativo com base no valor investido e no valor atual
    }
    // I - Interface Segregation: interfaces específicas evitam obrigar classes a implementar
    // métodos desnecessários
    // Interface específica: ativos que possuem data de vencimento
    public interface IAtivoComVencimento
    {
        DateTime DataVencimento { get; }
        int DiasParaVencimento();
    }

        // Interface específica: ativos que geram renda periódica como dividendos, distribuições etc.
    public interface IGeradorDeRenda
    {
        decimal CalcularRendaPeriodica();
        string Periodicidade { get; } // Ex.: "Mensal", "Trimestral", "Anual"
    }
        
    // Interface específica: ativos negociáveis em mercado
    public interface IAtivoNegociavel 
    {
        decimal PrecoDeCompra { get; }
        decimal PrecoMercado { get; }
        decimal VariacaoDiaria { get; } // percentual de variação do dia
    }

    
    public class Acao : IAtivoFinanceiro, IAtivoNegociavel, IGeradorDeRenda
    {   
        // S - Single Responsibility: classe Acao só modela ações
        // O - Open/Closed: podemos criar novas classes de ativos sem alterar Acao
        // L - Liskov Substitution: Acao pode substituir IAtivoFinanceiro em Portfolio<T>

        //propriedades da interface IAtivoFinanceiro
        public string Nome { get; }
        public int Quantidade { get; }

        // Gera calculo automatico
        public decimal ValorInvestido => Quantidade * PrecoDeCompra;
        public decimal ValorAtual => Quantidade * PrecoMercado;

        //propriedades da interface IAtivoNegociavel
        public decimal PrecoDeCompra { get; private set; }
        public decimal PrecoMercado { get; private set; }
        public decimal VariacaoDiaria { get; private set; }

        // Propriedades da interface IGeradorDeRenda
        public string Periodicidade { get; private set; }
        public decimal DividendosRecebidos { get; private set; }

        

        /// <summary>
        /// Calcula o valor do dividendo recebido por cada ação o dividend yield (dividendo pago/preço de mercado).
        /// Caso a quantidade seja zero, retorna 0 para evitar divisão por zero.
        /// </summary>
        public decimal DividendoPorAcao => Quantidade == 0 ? 0 : DividendosRecebidos / Quantidade;
        public decimal DividendYield => ValorAtual == 0 ? 0 : (DividendosRecebidos / ValorAtual) * 100;


        ///<summary>
        /// Permite definir os principais atributos de uma ação no momento da criação.
        /// </summary>
        public Acao(string nome, int quantidade, decimal precoDeCompra, decimal precoMercado, decimal variacaoDiaria, string periodicidade, decimal dividendosRecebidos)
        {
            Nome = nome;
            Quantidade = quantidade;
            PrecoDeCompra = precoDeCompra;
            PrecoMercado = precoMercado;
            VariacaoDiaria = variacaoDiaria;
            Periodicidade = periodicidade;
            DividendosRecebidos = dividendosRecebidos;
        }

        // Calcula a rentabilidade
        public decimal CalcularRentabilidade()
        {
            if (ValorInvestido == 0) return 0;
            return ((ValorAtual + DividendosRecebidos - ValorInvestido) / ValorInvestido) * 100;
        }
        // Calcula a renda periodica
        public decimal CalcularRendaPeriodica()
        {
            return Quantidade * DividendoPorAcao;
        }
    }


    ///<summary>
    ///Representa uma titulo de renda fixa  que um ativo financeiro e tem um vencimento
    ///que implementa contrato  com as interface IAtivoFinanceiro, IAtivoComVencimento
    /// </summary>

    public class TituloRendaFixa : IAtivoFinanceiro, IAtivoComVencimento
    {
        public string Nome { get; }
        public int Quantidade { get; }
        public decimal ValorInvestido { get; }
        public decimal TaxaAnual { get; }
        public DateTime DataAplicacao { get; }
        public DateTime DataVencimento { get; }

        //Inicializa uma nova instância da classe TituloRendaFixa e define os principais atributo
        public TituloRendaFixa(string nome, int quantidade, decimal valorInvestido, decimal taxaAnual, DateTime dataAplicacao, DateTime dataVencimento)
        {
            Nome = nome;
            Quantidade = quantidade;
            ValorInvestido = valorInvestido;
            TaxaAnual = taxaAnual;
            DataAplicacao = dataAplicacao;
            DataVencimento = dataVencimento;
        }

        // --- Métodos auxiliares com data de referência  ---
        public int DiasDecorridos(DateTime dataReferencia) => (dataReferencia - DataAplicacao).Days;
        public int DiasParaVencimento(DateTime dataReferencia) => (DataVencimento - dataReferencia).Days;

        //Calcula a rentabilidade para oativo especifico
        public decimal CalcularRentabilidade(DateTime dataReferencia)
        {
            int dias = DiasDecorridos(dataReferencia);

            // Se TaxaAnual já está em percentual (ex: 10 para 10%), não multiplique por 100
            decimal taxaPercentual = TaxaAnual;

            return taxaPercentual * (dias / 365m);

        }

        //calcula o valor atual do titulo
        public decimal CalculaValorAtual(DateTime dataReferencia)
        {
            decimal rentabilidade = CalcularRentabilidade(dataReferencia);
            return ValorInvestido * (1 + rentabilidade / 100);
        }

        // --- Implementação da interface (usa DateTime.Now) ---
        public int DiasParaVencimento() => (DataVencimento - DateTime.Now).Days;
        public decimal CalcularRentabilidade()
        {
            return CalcularRentabilidade(DateTime.Now);
        }

        public decimal ValorAtual
        {
            get
            {
                return CalculaValorAtual(DateTime.Now);
            }
        }

    }


    public class FundoInvestimento : IAtivoFinanceiro, IGeradorDeRenda
    {
        public string Nome { get; }
        public int Quantidade { get; } // quantidade de cotas
        public decimal ValorCotaCompra { get; }
        public decimal ValorCotaAtual { get; private set; }
        public decimal TaxaAdministracao { get; } // apenas informativa
        public decimal RendimentoPorCota { get; }
        public string Periodicidade { get; }

        public FundoInvestimento(string nome, int quantidadeCotas, decimal valorCotaCompra, decimal valorCotaAtual,
                                 decimal taxaAdministracao, decimal rendimentoPorCota, string periodicidade)
        {
            Nome = nome;
            Quantidade = quantidadeCotas;
            ValorCotaCompra = valorCotaCompra;
            ValorCotaAtual = valorCotaAtual;
            TaxaAdministracao = taxaAdministracao;
            RendimentoPorCota = rendimentoPorCota;
            Periodicidade = periodicidade;
        }

        // Valor originalmente investido
        public decimal ValorInvestido => Quantidade * ValorCotaCompra;

        // Valor atual
        public decimal ValorAtual => Quantidade * ValorCotaAtual;

        // Rentabilidade pela variação da cota
        public decimal CalcularRentabilidade()
        {
            if (ValorCotaCompra == 0) return 0;
            return ((ValorCotaAtual - ValorCotaCompra) / ValorCotaCompra) * 100;
        }

        // Renda periódica
        public decimal CalcularRendaPeriodica()
        {
            return Quantidade * RendimentoPorCota;
        }
    }

    // D - Dependency Inversion: Portfolio depende da abstração IAtivoFinanceiro, não das classes concretas
    public class Portfolio<T> where T : IAtivoFinanceiro
    {
        private List<T> ativos = new List<T>();

        public void Adicionar(T ativo) => ativos.Add(ativo);

        public decimal ValorTotal => ativos.Sum(a => a.ValorAtual);

        public decimal RentabilidadeMediaPonderada()
        {
            decimal valorTotal = ValorTotal;
            if (valorTotal == 0) return 0;

            return ativos.Sum(a => a.CalcularRentabilidade() * (a.ValorAtual / valorTotal));
        }

        public IEnumerable<T> FiltrarPor(Func<T, bool> predicado)
        {
            return ativos.Where(predicado);
        }

        public IEnumerable<T> Ativos => ativos;
    }

    public static class Relatorio
    {
        public static void Gerar<T>(Portfolio<T> portfolio) where T : IAtivoFinanceiro
        {
            foreach (var ativo in portfolio.Ativos)
            {
                Type tipo = ativo.GetType();
                Console.WriteLine($"Classe: {tipo.Name}");

                // Reflection: obtém todas as propriedades públicas da classe dinamicamente
                // Listar propriedades públicas com formatação adequada
                foreach (var prop in tipo.GetProperties())
                {
                    var valor = prop.GetValue(ativo); // Reflection: acessa o valor da propriedade em tempo de execução

                    // DividendYield em percentual
                    if (prop.Name == "DividendYield" && valor is decimal dy)
                    {
                        Console.WriteLine($"{prop.Name}: {dy:F2}%");
                    }
                    // Valores monetários
                    else if (valor is decimal dec)
                    {
                        Console.WriteLine($"{prop.Name}: {dec:C2}");
                    }
                    else if (valor is double dbl)
                    {
                        Console.WriteLine($"{prop.Name}: {dbl.ToString("C2")}");
                    }
                    else if (valor is int i)
                    {
                        Console.WriteLine($"{prop.Name}: {i}");
                    }
                    else
                    {
                        Console.WriteLine($"{prop.Name}: {valor}");
                    }
                }

                // Reflection: obtém o método "CalcularRentabilidade" dinamicamente
                // Chama o método
                var metodoRent = tipo.GetMethod("CalcularRentabilidade", Type.EmptyTypes);
                if (metodoRent != null)
                {
                    var rentabilidade = metodoRent.Invoke(ativo, null); // Reflection: invoca o método em tempo de execuçã
                    Console.WriteLine($"Rentabilidade: {Convert.ToDecimal(rentabilidade):F2}%");
                }

                // Reflection: descobre interfaces implementadas dinamicamente
                var interfaces = tipo.GetInterfaces();

                foreach (var iface in interfaces)
                {
                    // Reflection: obtém método "DiasParaVencimento" da interface
                    if (iface == typeof(IAtivoComVencimento))
                    {
                        var metodo = iface.GetMethod("DiasParaVencimento");
                        var dias = metodo.Invoke(ativo, null);        // invocação dinâmica
                        Console.WriteLine($"Dias para vencimento: {dias} dias");
                    }

                    // Reflection: obtém método e propriedade da interface IGeradorDeRenda
                    if (iface == typeof(IGeradorDeRenda))
                    {
                        var metodoRenda = iface.GetMethod("CalcularRendaPeriodica");
                        var renda = metodoRenda.Invoke(ativo, null);
                        var propPeriodicidade = iface.GetProperty("Periodicidade");
                        var periodicidade = propPeriodicidade.GetValue(ativo);    // acesso dinâmico
                        Console.WriteLine($"Renda periódica ({periodicidade}): {Convert.ToDecimal(renda):C2}");
                    }

                    //Tentei criar o metodo ativo negociavel, porem ele duplica as saidas. NÂO conseguiresolver
                    // IAtivoNegociavel
                    if (iface == typeof(IAtivoNegociavel))
                    {
                        //var propCompra = iface.GetProperty("PrecoDeCompra");
                        //var propMercado = iface.GetProperty("PrecoMercado");
                        //var propVariacao = iface.GetProperty("VariacaoDiaria");

                        //Console.WriteLine($"Preço de compra: {Convert.ToDecimal(propCompra.GetValue(ativo)):C2}");
                        //Console.WriteLine($"Preço de mercado: {Convert.ToDecimal(propMercado.GetValue(ativo)):C2}");
                        //Console.WriteLine($"Variação diária: {Convert.ToDecimal(propVariacao.GetValue(ativo)):F2}%");
                    }
                }

                Console.WriteLine(new string('-', 60));
            }

            Console.WriteLine($"Valor total do portfólio: {portfolio.ValorTotal:C2}");
            Console.WriteLine($"Rentabilidade média ponderada: {portfolio.RentabilidadeMediaPonderada():F2}%\n");
        }
    }



    internal class Program
     {
            static void Main(string[] args)
            {

                // Exemplo de título de renda fixa com datas fixas
                DateTime dataAplicacao = new DateTime(2026, 1, 1);
                DateTime dataReferencia = dataAplicacao.AddDays(180); // simula execução após 180 dias
                DateTime dataVencimento = new DateTime(2027, 8, 15);

                // Primeira ação: Petrobras
                Acao acaoPetrobras = new Acao(
                    nome: "Petrobras",
                    quantidade: 100,
                    precoDeCompra: 30m,
                    precoMercado: 32.50m,
                    variacaoDiaria: 1.2m,
                    periodicidade: "Trimestral",
                    dividendosRecebidos: 100m
                );

                // Segunda ação: Vale
                Acao acaoVale = new Acao(
                    nome: "Vale",
                    quantidade: 50,
                    precoDeCompra: 70m,
                    precoMercado: 75m,
                    variacaoDiaria: 0.8m,
                    periodicidade: "Semestral",
                    dividendosRecebidos: 200m
                );
            
            // Exemplo de título de renda fixa
            TituloRendaFixa tituloTesouro2028 = new TituloRendaFixa(
                    nome: "Tesouro Prefixado 2028",
                    quantidade: 100,
                    valorInvestido: 5000m,
                    taxaAnual: 10m, // 10% ao ano
                    dataAplicacao: new DateTime(2024, 1, 1),
                    dataVencimento: new DateTime(2028, 1, 1)
                );

                // Exemplo de título de renda fixa
                TituloRendaFixa tituloTesouro2032 = new TituloRendaFixa(
                     nome: "Tesouro Prefixado 2032",
                     quantidade: 1000,
                     valorInvestido: 5000m,
                     taxaAnual: 12m, // 12% ao ano
                     dataAplicacao: new DateTime(2026, 1, 1),
                     dataVencimento: new DateTime(2032, 1, 1)
                 );

                TituloRendaFixa tituloCDB = new TituloRendaFixa(
                    nome: "CDB Prefixado 10% a.a.",
                    quantidade: 1,
                    valorInvestido: 10000m,
                    taxaAnual: 10m,
                    dataAplicacao: dataAplicacao,
                    dataVencimento: dataVencimento
                );

                FundoInvestimento fundoXPTO = new FundoInvestimento(
                    nome: "Fundo Multimercado XPTO",
                    quantidadeCotas: 100,
                    valorCotaCompra: 80m,
                    valorCotaAtual: 84.20m,
                    taxaAdministracao: 2m, // apenas informativa
                    rendimentoPorCota: 1.50m,
                    periodicidade: "Mensal"
                );

                FundoInvestimento fundoZPTO = new FundoInvestimento(
                    nome: "Fundo Multimercado ZPTO",
                    quantidadeCotas: 1000,
                    valorCotaCompra: 25m,
                    valorCotaAtual: 34.20m,
                    taxaAdministracao: 2m, // apenas informativa
                    rendimentoPorCota: 0.50m,
                    periodicidade: "Mensal"
                );
            
                // Lista de ativos
                var carteira = new List<Acao> { acaoPetrobras, acaoVale };

                var carteiraTituloRF = new List<TituloRendaFixa> { tituloTesouro2028, tituloTesouro2032, tituloCDB };

                var carteiraFI = new List<FundoInvestimento> { fundoXPTO, fundoZPTO };
                
                Console.WriteLine("=== Carteira de Ações ===\n");

                foreach (var acao in carteira)
                {
                    Console.WriteLine($"Ativo: {acao.Nome}");
                    Console.WriteLine($"Quantidade: {acao.Quantidade}");
                    Console.WriteLine($"Investido: {acao.ValorInvestido:C}");
                    Console.WriteLine($"Atual: {acao.ValorAtual:C}");
                    Console.WriteLine($"Div. Recebidos: {acao.DividendosRecebidos:C}");
                    Console.WriteLine($"Div/Ação: {acao.DividendoPorAcao:C}");
                    Console.WriteLine($"Rentabilidade: {acao.CalcularRentabilidade():F2}%");
                    Console.WriteLine($"Renda Periódica: {acao.CalcularRendaPeriodica():C}");
                    Console.WriteLine(new string('-', 60)); // separador visual
                }

            Console.WriteLine("Precione para continuar");
            Console.ReadKey();
            Console.WriteLine("=== Carteira de Fundos de Renda Fisica===\n");

                foreach (var titulo in carteiraTituloRF)
                {
                    Console.WriteLine($"Ativo: {titulo.Nome}");
                    Console.WriteLine($"Quantidade: {titulo.Quantidade} cotas");
                    Console.WriteLine($"Investido: {titulo.ValorInvestido:C}");
                    Console.WriteLine($"Taxa Anual: {titulo.TaxaAnual:F2}%");
                    Console.WriteLine($"Atual: {titulo.CalculaValorAtual(dataReferencia):C}");
                    Console.WriteLine($"Rentabilidade: {titulo.CalcularRentabilidade(dataReferencia):F2}%");
                    Console.WriteLine($"Dias decorridos: {titulo.DiasDecorridos(dataReferencia)} dias");
                    Console.WriteLine($"Dias para vencimento: {titulo.DiasParaVencimento(dataReferencia)} dias");

                Console.WriteLine(new string('-', 60)); // separador visual
                }

            Console.WriteLine("Precione para continuar");
            Console.ReadKey();

            Console.WriteLine("=== Carteira de Fundos de Investimento ===\n");
                foreach (var fundo in carteiraFI)
                {
                    Console.WriteLine($"Ativo: {fundo.Nome}");
                    Console.WriteLine($"Quantidade: {fundo.Quantidade} cotas");
                    Console.WriteLine($"Valor da cota na compra: {fundo.ValorCotaCompra:C}");
                    Console.WriteLine($"Valor atual da cota: {fundo.ValorCotaAtual:C}");
                    Console.WriteLine($"Taxa de administração: {fundo.TaxaAdministracao:F2}%");
                    Console.WriteLine($"Valor Investido: {fundo.ValorInvestido:C}");
                    Console.WriteLine($"Valor Atual: {fundo.ValorAtual:C}");
                    Console.WriteLine($"Rentabilidade: {fundo.CalcularRentabilidade():F2}%");
                    Console.WriteLine($"Renda Periódica ({fundo.Periodicidade}): {fundo.CalcularRendaPeriodica():C}");
                    Console.WriteLine(new string('-', 60)); // separador visual
                }
                Console.ReadLine();

            Console.WriteLine("Precione para continuar");
            Console.ReadKey();

            Console.WriteLine("RELATORIO CONSOLIDADO");
            Console.WriteLine(new string('-', 60));
            // Criar portfólios
            var portfolioAcoes = new Portfolio<Acao>();
            portfolioAcoes.Adicionar(acaoPetrobras);
            portfolioAcoes.Adicionar(acaoVale);

            var portfolioRendaFixa = new Portfolio<TituloRendaFixa>();
            portfolioRendaFixa.Adicionar(tituloTesouro2028);
            portfolioRendaFixa.Adicionar(tituloTesouro2032);
            portfolioRendaFixa.Adicionar(tituloCDB);

            var portfolioFundos = new Portfolio<FundoInvestimento>();
            portfolioFundos.Adicionar(fundoXPTO);
            portfolioFundos.Adicionar(fundoZPTO);

            // Relatório de ações
            Console.WriteLine("=== Carteira de Ações ===\n");
            foreach (var acao in portfolioAcoes.Ativos)
            {
                Console.WriteLine($"Ativo: {acao.Nome}");
                Console.WriteLine($"Quantidade: {acao.Quantidade}");
                Console.WriteLine($"Investido: {acao.ValorInvestido:C}");
                Console.WriteLine($"Atual: {acao.ValorAtual:C}");
                Console.WriteLine($"Rentabilidade: {acao.CalcularRentabilidade():F2}%");
                Console.WriteLine(new string('-', 60));
            }
            Console.WriteLine($"Valor total da carteira de ações: {portfolioAcoes.ValorTotal:C}");
            Console.WriteLine($"Rentabilidade média ponderada: {portfolioAcoes.RentabilidadeMediaPonderada():F2}%\n");
            Console.ReadLine();
            Console.WriteLine("Precione para continuar");
            Console.ReadKey();

            // Relatório de renda fixa
            Console.WriteLine("=== Carteira de Renda Fixa ===\n");
            foreach (var titulo in portfolioRendaFixa.Ativos)
            {
                Console.WriteLine($"Ativo: {titulo.Nome}");
                Console.WriteLine($"Investido: {titulo.ValorInvestido:C}");
                Console.WriteLine($"Atual: {titulo.CalculaValorAtual(dataReferencia):C}");
                Console.WriteLine($"Rentabilidade: {titulo.CalcularRentabilidade(dataReferencia):F2}%");
                Console.WriteLine(new string('-', 60));
            }
            Console.WriteLine($"Valor total da carteira de renda fixa: {portfolioRendaFixa.ValorTotal:C}");
            Console.WriteLine($"Rentabilidade média ponderada: {portfolioRendaFixa.RentabilidadeMediaPonderada():F2}%\n");
            Console.ReadLine();
            Console.WriteLine("Precione para continuar");
            Console.ReadKey();
            // Relatório de fundos
            Console.WriteLine("=== Carteira de Fundos de Investimento ===\n");
            foreach (var fundo in portfolioFundos.Ativos)
            {
                Console.WriteLine($"Ativo: {fundo.Nome}");
                Console.WriteLine($"Investido: {fundo.ValorInvestido:C}");
                Console.WriteLine($"Atual: {fundo.ValorAtual:C}");
                Console.WriteLine($"Rentabilidade: {fundo.CalcularRentabilidade():F2}%");
                Console.WriteLine(new string('-', 60));
            }
            Console.WriteLine($"Valor total da carteira de fundos: {portfolioFundos.ValorTotal:C}");
            Console.WriteLine($"Rentabilidade média ponderada: {portfolioFundos.RentabilidadeMediaPonderada():F2}%\n");

            Console.ReadLine();

            Console.WriteLine("Precione para continuar");
            Console.ReadKey();

            Console.WriteLine("Relatoirio Dinamicos");
            Console.WriteLine(new string('-', 60));

            Console.WriteLine("=== Relatório Dinâmico: Ações ===");
            Relatorio.Gerar(portfolioAcoes);

            Console.WriteLine("=== Relatório Dinâmico: Renda Fixa ===");
            Relatorio.Gerar(portfolioRendaFixa);

            Console.WriteLine("=== Relatório Dinâmico: Fundos ===");
            Relatorio.Gerar(portfolioFundos);

        }
    }
    
}