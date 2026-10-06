using Conde.Autovalores;

namespace Conde.Autovalores.Medidor;

/// <summary>
/// As medidas. Tudo contado em iteracoes, residuos e identidades conferidas:
/// nenhuma depende de relogio nem de maquina.
/// </summary>
public static class Programa
{
    public static int Main(string[] argumentos)
    {
        var qual = argumentos.Length > 0 ? argumentos[0] : "tudo";
        switch (qual)
        {
            case "juiz": Juiz(); break;
            case "potencia": Potencias(); break;
            case "jacobi": Jacobis(); break;
            case "qr": Qrs(); break;
            case "deslocamento": Deslocamentos(); break;
            case "condicionamento": Condicionamento(); break;
            case "tudo": Juiz(); Potencias(); Jacobis(); Qrs(); Deslocamentos(); Condicionamento(); break;
            default:
                Console.Error.WriteLine("medidas: juiz, potencia, jacobi, qr, deslocamento, condicionamento, tudo");
                return 1;
        }
        return 0;
    }

    /// <summary>Os autovalores exatos e as duas identidades que confirmam.</summary>
    private static void Juiz()
    {
        Console.WriteLine("== os autovalores exatos, e as duas identidades que conferem ==");
        Console.WriteLine();
        Console.WriteLine($"{"matriz",-16}{"ordem",7}{"autovalores",34}{"erro do traco",15}{"erro do det",14}");

        foreach (var (nome, matriz) in Casos.Todas())
        {
            var exatos = Exata.De(matriz);

            if (exatos is null)
            {
                Console.WriteLine($"{nome,-16}{matriz.Ordem,7}{"nenhum real",34}{"-",15}{"-",14}");
                continue;
            }

            Console.WriteLine($"{nome,-16}{matriz.Ordem,7}{string.Join("  ", exatos.Select(x => $"{x,9:N5}")),34}" +
                              $"{Residuo.ErroDoTraco(matriz, exatos),15:E1}{Residuo.ErroDoDeterminante(matriz, exatos),14:E1}");
        }

        Console.WriteLine();
        Console.WriteLine("a soma dos autovalores e o TRACO e o produto e o DETERMINANTE, sempre. Sao");
        Console.WriteLine("as duas conferencias mais baratas que existem: elas nao dizem quais");
        Console.WriteLine("autovalores sao, e dizem na hora se o conjunto esta errado");
        Console.WriteLine();
        Console.WriteLine("a ROTACAO de noventa graus nao tem autovalor real nenhum, e isso nao e");
        Console.WriteLine("defeito: ela nao deixa direcao nenhuma no lugar, entao nao ha autovetor");
        Console.WriteLine("real para metodo real nenhum achar");
        Console.WriteLine();

        Console.WriteLine($"  e o polinomio caracteristico avaliado nos autovalores achados:");
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var exatos = Exata.De(matriz)!;
            var pior = exatos.Max(x => Math.Abs(Exata.Caracteristico(matriz, x)));

            Console.WriteLine($"    {nome,-16}{pior,12:E2}");
        }

        Console.WriteLine();
        Console.WriteLine("  ele e zero exatamente nos autovalores, e e isso que a coluna mostra: a");
        Console.WriteLine("  formula fechada nao esta sendo conferida contra si mesma");
        Console.WriteLine();
    }

    /// <summary>O metodo da potencia, e o caso em que ele nao converge.</summary>
    private static void Potencias()
    {
        Console.WriteLine("== o metodo da potencia: so o dominante, e nem sempre ==");
        Console.WriteLine();
        Console.WriteLine($"{"matriz",-16}{"dominante",12}{"achado",12}{"passos",10}{"razao",10}{"residuo",12}{"convergiu",12}");

        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var exatos = Exata.De(matriz)!;
            var dominante = exatos.OrderByDescending(Math.Abs).First();
            var r = Potencia.Dominante(matriz, limite: 20_000);

            Console.WriteLine($"{nome,-16}{dominante,12:N5}{r.Autovalor,12:N5}{r.Passos,10:N0}" +
                              $"{Potencia.Razao(exatos),10:N4}{Residuo.De(matriz, r.Autovalor, r.Autovetor),12:E1}" +
                              $"{(r.Convergiu ? "sim" : "NAO"),12}");
        }

        Console.WriteLine();
        Console.WriteLine("a coluna da razao e o segundo maior autovalor em modulo dividido pelo maior,");
        Console.WriteLine("e e ela que dita a velocidade: o erro encolhe por esse fator a cada passo");
        Console.WriteLine();
        Console.WriteLine("no ESPELHO a razao e UM, e o metodo nao converge nunca. Os autovalores sao");
        Console.WriteLine("1 e -1, de mesmo modulo, entao as duas parcelas do vetor nunca se separam e");
        Console.WriteLine("ele oscila entre duas direcoes para sempre");
        Console.WriteLine();
        Console.WriteLine("e olhe a linha do espelho inteira: ele diz que CONVERGIU, em dois passos,");
        Console.WriteLine("com residuo 3,8E-001. O criterio de parada e 'o quociente de Rayleigh parou");
        Console.WriteLine("de mudar', e no espelho ele nunca muda, porque trocar as duas coordenadas");
        Console.WriteLine("de lugar nao muda o quociente");
        Console.WriteLine();
        Console.WriteLine("'parou de mudar' nao e 'esta certo', e a coluna do RESIDUO e a unica que");
        Console.WriteLine("sabe a diferenca. Ela mede A vezes v menos L vezes v, que e a definicao, e");
        Console.WriteLine("nao depende de confiar em quem calculou");
        Console.WriteLine();

        Console.WriteLine($"  e a POTENCIA INVERSA acha QUALQUER autovalor, dado um palpite:");
        Console.WriteLine($"  {"matriz",-16}{"palpite",10}{"achado",12}{"passos",10}{"residuo",12}");

        var simetrica = Casos.Simetrica();
        foreach (var palpite in new[] { 0.5, 2.2, 5.5 })
        {
            var r = Potencia.Inversa(simetrica, palpite);
            Console.WriteLine($"  {"simetrica",-16}{palpite,10:N1}{r.Autovalor,12:N5}{r.Passos,10}" +
                              $"{Residuo.De(simetrica, r.Autovalor, r.Autovetor),12:E1}");
        }

        Console.WriteLine();
        Console.WriteLine("  e a ARMADILHA do vetor de partida:");

        var armadilha = Casos.LinhasIguais();
        var uns = new double[] { 1, 1 };
        var comUns = Potencia.Dominante(armadilha, uns);
        var comPadrao = Potencia.Dominante(armadilha);

        Console.WriteLine($"    a matriz tem autovalores 4 e 2, e as linhas somam dois");
        Console.WriteLine($"    comecando em (1, 1):   achou {comUns.Autovalor:N5} em {comUns.Passos} passo(s), " +
                          $"dizendo que convergiu: {comUns.Convergiu}");
        Console.WriteLine($"    comecando no padrao:   achou {comPadrao.Autovalor:N5} em {comPadrao.Passos} passos");
        Console.WriteLine();
        Console.WriteLine("    o vetor de todos iguais a um JA e autovetor, e do autovalor que NAO e o");
        Console.WriteLine("    dominante. O metodo fica parado nele para sempre e diz que convergiu");
        Console.WriteLine();
        Console.WriteLine("    comecar com todos iguais a um e o palpite mais natural do mundo");
        Console.WriteLine();

        Console.WriteLine("  um sobre uma diferenca pequena e um numero grande: deslocar a matriz pelo");
        Console.WriteLine("  palpite e inverter faz o autovalor mais proximo virar o dominante, e o");
        Console.WriteLine("  mesmo metodo que so achava o maior passa a achar qualquer um");
        Console.WriteLine();
    }

    /// <summary>Jacobi e a quantidade que so cai.</summary>
    private static void Jacobis()
    {
        Console.WriteLine("== Jacobi: a quantidade que so cai, rotacao a rotacao ==");
        Console.WriteLine();
        Console.WriteLine($"{"matriz",-22}{"ordem",7}{"rotacoes",10}{"erro contra o exato",22}{"so cai",10}");

        foreach (var (nome, matriz) in Casos.Simetricas())
        {
            var r = Jacobi.Diagonalizar(matriz);
            var exatos = Exata.De(matriz);
            var erro = exatos is null ? double.NaN : Residuo.Distancia(exatos, r.Autovalores);

            Console.WriteLine($"{nome,-22}{matriz.Ordem,7}{r.Rotacoes,10}{erro,22:E2}" +
                              $"{(Jacobi.SoCai(r.ForaDaDiagonal) ? "sim" : "NAO"),10}");
        }

        Console.WriteLine();
        Console.WriteLine("cada rotacao zera UM elemento de fora da diagonal e estraga um pouco os");
        Console.WriteLine("zeros anteriores. Mesmo assim a soma dos quadrados de fora da diagonal CAI");
        Console.WriteLine("em todas as rotacoes, sem excecao, e e isso que garante a convergencia");
        Console.WriteLine();

        var quatro = Casos.Simetrica4();
        var trilha = Jacobi.Diagonalizar(quatro).ForaDaDiagonal;

        Console.WriteLine($"  a soma de fora da diagonal, rotacao a rotacao, na simetrica de ordem 4:");
        for (var i = 0; i < Math.Min(12, trilha.Count); i++)
            Console.WriteLine($"    rotacao {i,2}   {trilha[i],14:E4}");

        Console.WriteLine();
        Console.WriteLine("  ela nao e argumento: e numero, e da para olhar cair");
        Console.WriteLine();
        Console.WriteLine("  e Jacobi so vale para matriz SIMETRICA, o que nao e limitacao pequena:");
        Console.WriteLine("  matriz simetrica tem todos os autovalores reais e uma base de autovetores");
        Console.WriteLine("  ortogonais, e matriz qualquer nao tem nem uma coisa nem outra");
        Console.WriteLine();
    }

    /// <summary>A fatoracao QR, e o estrago do Gram-Schmidt classico.</summary>
    private static void Qrs()
    {
        Console.WriteLine("== a fatoracao QR: a mesma conta em outra ordem muda tudo ==");
        Console.WriteLine();
        Console.WriteLine($"{"matriz",-22}{"classico: Q nao ortogonal",28}{"modificado",14}{"razao",14}");

        foreach (var (nome, matriz) in Casos.Todas().Append(("quase dependente", Casos.QuaseDependente())))
        {
            var classico = Qr.DesvioDeOrtogonalidade(Qr.FatorarClassico(matriz).Q);
            var modificado = Qr.DesvioDeOrtogonalidade(Qr.Fatorar(matriz).Q);
            var razao = modificado > 0 ? classico / modificado : 1;

            Console.WriteLine($"{nome,-22}{classico,28:E2}{modificado,14:E2}{razao,14:N1}");
        }

        Console.WriteLine();
        Console.WriteLine("as duas versoes fazem as MESMAS contas em outra ordem. A classica projeta");
        Console.WriteLine("na coluna original; a modificada projeta no que sobrou dela depois das");
        Console.WriteLine("projecoes anteriores");
        Console.WriteLine();
        Console.WriteLine("na matriz quase dependente, isso e a diferenca entre Q quase ortogonal e Q");
        Console.WriteLine("que nao e ortogonal nem de longe. E a mesma algebra");
        Console.WriteLine();

        Console.WriteLine($"  e as duas reconstroem a matriz original igualmente bem:");
        Console.WriteLine($"  {"matriz",-22}{"classico",14}{"modificado",14}");

        foreach (var (nome, matriz) in new[] { ("quase dependente", Casos.QuaseDependente()), ("simetrica", Casos.Simetrica()) })
            Console.WriteLine($"  {nome,-22}{Qr.ErroDaFatoracao(matriz, Qr.FatorarClassico(matriz)),14:E2}" +
                              $"{Qr.ErroDaFatoracao(matriz, Qr.Fatorar(matriz)),14:E2}");

        Console.WriteLine();
        Console.WriteLine("  esse e o detalhe que engana: Q vezes R devolve a matriz nas duas, entao a");
        Console.WriteLine("  conferencia obvia passa. O que quebra e a ORTOGONALIDADE de Q, e so quem");
        Console.WriteLine("  olha para ela ve");
        Console.WriteLine();
    }

    /// <summary>O deslocamento, que e a linha que faz o QR valer a pena.</summary>
    private static void Deslocamentos()
    {
        Console.WriteLine("== o deslocamento: uma linha, ordens de grandeza ==");
        Console.WriteLine();
        Console.WriteLine($"{"matriz",-16}{"sem deslocamento",20}{"com",10}{"razao",12}{"erro contra o exato",22}");

        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var (sem, com) = Deslocamento.Comparar(matriz);
            var exatos = Exata.De(matriz)!;
            var achados = Deslocamento.Iterar(matriz).Autovalores;

            Console.WriteLine($"{nome,-16}{sem,20:N0}{com,10:N0}{(double)sem / com,12:N1}" +
                              $"{Residuo.Distancia(exatos, achados),22:E2}");
        }

        Console.WriteLine();
        Console.WriteLine("o laco sem deslocamento converge pela razao entre autovalores vizinhos,");
        Console.WriteLine("elevada ao numero de voltas. Com dois autovalores parecidos isso e quase um,");
        Console.WriteLine("e ele gira milhares de vezes");
        Console.WriteLine();
        Console.WriteLine("com o deslocamento, a velocidade passa a depender das DISTANCIAS ate o");
        Console.WriteLine("deslocamento escolhido, e escolhendo ele perto de um autovalor a razao vira");
        Console.WriteLine("quase zero");
        Console.WriteLine();
        Console.WriteLine("o deslocamento usado e o de Wilkinson, que olha o bloco dois por dois do");
        Console.WriteLine("canto. Pegar simplesmente o ultimo elemento da diagonal tambem funciona e e");
        Console.WriteLine("pior: na rotacao, em que o canto e zero, ele nao desloca nada");
        Console.WriteLine();
    }

    /// <summary>O condicionamento, que e da matriz e nao do metodo.</summary>
    private static void Condicionamento()
    {
        Console.WriteLine("== o condicionamento e da MATRIZ, nao do metodo ==");
        Console.WriteLine();
        Console.WriteLine($"{"matriz",-16}{"simetrica",12}{"cutucada de",14}{"autovalores andam",20}{"razao",12}");

        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
            foreach (var tamanho in new[] { 1e-10 })
            {
                var andou = Residuo.Sensibilidade(matriz, tamanho);

                Console.WriteLine($"{nome,-16}{(matriz.Simetrica() ? "sim" : "nao"),12}{tamanho,14:E0}" +
                                  $"{andou,20:E2}{andou / tamanho,12:N0}");
            }

        Console.WriteLine();
        Console.WriteLine("nas matrizes SIMETRICAS a ultima coluna e um: uma perturbacao de tamanho e");
        Console.WriteLine("move os autovalores no maximo e, e nao ha metodo ruim que estrague isso nem");
        Console.WriteLine("metodo bom que melhore");
        Console.WriteLine();
        Console.WriteLine("no bloco de JORDAN ela e cem mil: a perturbacao de 1e-10 move os autovalores");
        Console.WriteLine("em 1e-5, que e a RAIZ dela. Autovalor repetido sem base de autovetores faz o");
        Console.WriteLine("problema ser mal condicionado, e nenhum metodo conserta, porque nao e");
        Console.WriteLine("defeito de metodo");
        Console.WriteLine();

        Console.WriteLine($"  o bloco de Jordan, cutucado em tamanhos diferentes:");
        Console.WriteLine($"  {"cutucada",12}{"andou",14}{"raiz da cutucada",20}");

        foreach (var tamanho in new[] { 1e-4, 1e-6, 1e-8, 1e-10, 1e-12 })
            Console.WriteLine($"  {tamanho,12:E0}{Residuo.Sensibilidade(Casos.Jordan(), tamanho),14:E2}{Math.Sqrt(tamanho),20:E2}");

        Console.WriteLine();
        Console.WriteLine("  as duas ultimas colunas sao o mesmo numero. O deslocamento dos autovalores");
        Console.WriteLine("  e a RAIZ da perturbacao, e nao a perturbacao, e e por isso que um erro de");
        Console.WriteLine("  arredondamento de 1e-16 pode virar um erro de 1e-8 na resposta");
        Console.WriteLine();
    }
}
