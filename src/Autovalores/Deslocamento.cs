namespace Conde.Autovalores;

/// <summary>
/// O DESLOCAMENTO: a linha que transforma o algoritmo QR de lento em rapido.
///
/// A ideia e simples e o efeito e desproporcional. Em vez de fatorar A, fatore
/// A menos s vezes a identidade, multiplique ao contrario e some s de volta. Os
/// autovalores de A nao mudam, e a velocidade de convergencia passa a depender
/// das distancias ate s, e nao das razoes entre autovalores.
///
/// Escolhendo s perto de um autovalor, a razao que manda vira quase zero e
/// aquele autovalor converge em poucas voltas. A escolha usada aqui e o
/// deslocamento de Wilkinson, que olha o bloco dois por dois do canto e pega o
/// autovalor dele mais proximo do ultimo elemento da diagonal.
///
/// A medida mostra o tamanho da diferenca: numa matriz com autovalores
/// parecidos, o laco sem deslocamento gira milhares de vezes e o com
/// deslocamento gira algumas dezenas.
/// </summary>
public static class Deslocamento
{
    public sealed record Resultado(double[] Autovalores, int Iteracoes, bool Convergiu);

    /// <summary>
    /// O deslocamento de WILKINSON: o autovalor do bloco dois por dois do canto
    /// que esta mais perto do ultimo elemento da diagonal.
    ///
    /// Pegar simplesmente o ultimo elemento da diagonal tambem funciona e e pior:
    /// numa matriz como a rotacao, em que o canto e zero, o deslocamento nulo
    /// nao desloca nada e o laco trava. O bloco dois por dois enxerga isso.
    /// </summary>
    public static double DeWilkinson(Matriz matriz, int ate)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        if (ate < 1) return matriz[0, 0];

        var a = matriz[ate - 1, ate - 1];
        var b = matriz[ate - 1, ate];
        var c = matriz[ate, ate - 1];
        var d = matriz[ate, ate];

        var meio = (a + d) / 2;
        var dentro = meio * meio - (a * d - b * c);

        if (dentro < 0) return d;

        var raiz = Math.Sqrt(dentro);
        var um = meio + raiz;
        var outro = meio - raiz;

        return Math.Abs(um - d) < Math.Abs(outro - d) ? um : outro;
    }

    /// <summary>
    /// O laco QR COM deslocamento, com deflacao.
    ///
    /// A deflacao e a outra metade da eficiencia: assim que o ultimo elemento
    /// abaixo da diagonal fica desprezivel, aquele autovalor esta pronto e o
    /// problema encolhe em uma ordem. Sem ela, o laco continua mexendo em
    /// numeros que ja convergiram.
    /// </summary>
    public static Resultado Iterar(Matriz matriz, double tolerancia = 1e-12, int limite = 10_000)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var n = matriz.Ordem;
        var atual = matriz;
        var achados = new List<double>();
        var voltas = 0;
        var ate = n - 1;

        while (ate > 0 && voltas < limite)
        {
            voltas++;

            // A linha INTEIRA abaixo da diagonal precisa estar desprezivel, e
            // nao so o elemento vizinho. Olhando so o vizinho, uma matriz que
            // ainda tem peso na primeira coluna da ultima linha e deflacionada
            // cedo demais, e o autovalor guardado nao e autovalor de nada: na
            // simetrica de ordem tres isso dava erro de 5,3E-001 contra a
            // formula exata, com o laco dizendo que tinha convergido.
            var ultimaLinha = 0.0;
            for (var j = 0; j < ate; j++) ultimaLinha = Math.Max(ultimaLinha, Math.Abs(atual[ate, j]));

            if (ultimaLinha < tolerancia * (Math.Abs(atual[ate, ate]) + Math.Abs(atual[ate - 1, ate - 1]) + 1e-300))
            {
                achados.Add(atual[ate, ate]);
                atual = Encolher(atual, ate);
                ate--;
                continue;
            }

            var s = DeWilkinson(atual, ate);
            var (q, r) = Qr.Fatorar(atual.MenosDiagonal(s));
            atual = r.Vezes(q).MenosDiagonal(-s);
        }

        for (var i = 0; i <= ate; i++) achados.Add(atual[i, i]);

        var saida = achados.ToArray();
        Array.Sort(saida);
        Array.Reverse(saida);

        return new Resultado(saida, voltas, ate == 0);
    }

    /// <summary>Tira a ultima linha e a ultima coluna, que ja convergiram.</summary>
    private static Matriz Encolher(Matriz matriz, int ate)
    {
        var m = new double[ate, ate];

        for (var i = 0; i < ate; i++)
            for (var j = 0; j < ate; j++)
                m[i, j] = matriz[i, j];

        return new Matriz(m);
    }

    /// <summary>
    /// Quantas voltas cada versao gasta, lado a lado, que e a medida que
    /// justifica o deslocamento.
    /// </summary>
    public static (int Sem, int Com) Comparar(Matriz matriz, double tolerancia = 1e-10)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var sem = Qr.Iterar(matriz, tolerancia, 50_000);
        var com = Iterar(matriz, tolerancia);

        return (sem.Iteracoes, com.Iteracoes);
    }
}
