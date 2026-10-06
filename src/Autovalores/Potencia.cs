namespace Conde.Autovalores;

/// <summary>
/// O METODO DA POTENCIA: multiplicar um vetor pela matriz, de novo e de novo.
///
/// A ideia e de uma linha. Todo vetor e uma mistura dos autovetores; multiplicar
/// pela matriz multiplica cada parcela pelo autovalor dela; entao a parcela do
/// MAIOR autovalor em modulo domina depois de alguns passos, e o vetor aponta
/// para o autovetor dominante.
///
/// E e so isso que ele acha: o dominante. A velocidade e a razao entre o maior e
/// o segundo maior em modulo, elevada ao numero de passos, e quando os dois sao
/// parecidos ele fica lento sem avisar.
///
/// O caso em que ele NAO FUNCIONA e o mais instrutivo: com dois autovalores de
/// mesmo modulo e sinais opostos, as duas parcelas nunca se separam e o vetor
/// oscila para sempre. A matriz nao tem defeito nenhum e o metodo nao converge,
/// e a medida mostra isso rodando dez mil passos.
/// </summary>
public static class Potencia
{
    public sealed record Resultado(double Autovalor, double[] Autovetor, int Passos, bool Convergiu);

    /// <summary>
    /// O autovalor dominante, pelo quociente de Rayleigh.
    ///
    /// O quociente vale v transposto vezes A vezes v, com v normalizado, e ele e
    /// melhor que olhar uma coordenada do vetor: numa matriz simetrica, ele
    /// acerta com o QUADRADO da precisao do autovetor.
    /// </summary>
    public static Resultado Dominante(Matriz matriz, double[]? inicio = null,
                                      double tolerancia = 1e-12, int limite = 100_000)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var v = Vetor.Normalizar(inicio ?? Padrao(matriz.Ordem));
        var anterior = double.NaN;

        for (var passo = 1; passo <= limite; passo++)
        {
            var proximo = matriz.Vezes(v);
            var norma = Vetor.Norma(proximo);

            if (norma < 1e-300) return new Resultado(0, v, passo, true);

            v = Vetor.Vezes(1 / norma, proximo);
            var rayleigh = Vetor.Interno(v, matriz.Vezes(v));

            if (!double.IsNaN(anterior) && Math.Abs(rayleigh - anterior) < tolerancia)
                return new Resultado(rayleigh, v, passo, true);

            anterior = rayleigh;
        }

        return new Resultado(anterior, v, limite, false);
    }

    /// <summary>
    /// O vetor de partida: nao pode ser nulo e nao deve ser um autovetor.
    ///
    /// Comecar com todos iguais a um e tentador e e uma armadilha: numa matriz
    /// simetrica com linhas de mesma soma, esse vetor JA e um autovetor, e o
    /// metodo devolve ele na primeira iteracao achando que convergiu para o
    /// dominante. As coordenadas desiguais evitam isso.
    /// </summary>
    public static double[] Padrao(int n)
    {
        var v = new double[n];
        for (var i = 0; i < n; i++) v[i] = 1.0 / (i + 2);

        return v;
    }

    /// <summary>
    /// A POTENCIA INVERSA com deslocamento: achar o autovalor mais PROXIMO de um
    /// palpite.
    ///
    /// Aplicar o metodo da potencia a (A - s*I) elevado a menos um faz o
    /// autovalor mais perto de s virar o dominante, porque um sobre uma
    /// diferenca pequena e um numero grande. Com isso, o mesmo metodo que so
    /// achava o maior passa a achar qualquer um, desde que se saiba por onde
    /// procurar.
    /// </summary>
    public static Resultado Inversa(Matriz matriz, double deslocamento,
                                    double tolerancia = 1e-12, int limite = 10_000)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var deslocada = matriz.MenosDiagonal(deslocamento);
        var v = Vetor.Normalizar(Padrao(matriz.Ordem));
        var anterior = double.NaN;

        for (var passo = 1; passo <= limite; passo++)
        {
            var resolvido = Resolver(deslocada, v);
            if (resolvido is null) return new Resultado(deslocamento, v, passo, true);

            var norma = Vetor.Norma(resolvido);
            if (norma < 1e-300) return new Resultado(deslocamento, v, passo, true);

            v = Vetor.Vezes(1 / norma, resolvido);
            var rayleigh = Vetor.Interno(v, matriz.Vezes(v));

            if (!double.IsNaN(anterior) && Math.Abs(rayleigh - anterior) < tolerancia)
                return new Resultado(rayleigh, v, passo, true);

            anterior = rayleigh;
        }

        return new Resultado(anterior, v, limite, false);
    }

    /// <summary>Resolve o sistema por eliminacao com pivotamento parcial.</summary>
    public static double[]? Resolver(Matriz matriz, double[] lado)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        ArgumentNullException.ThrowIfNull(lado);

        var n = matriz.Ordem;
        var a = new double[n, n + 1];

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++) a[i, j] = matriz[i, j];
            a[i, n] = lado[i];
        }

        for (var coluna = 0; coluna < n; coluna++)
        {
            var melhor = coluna;
            for (var linha = coluna + 1; linha < n; linha++)
                if (Math.Abs(a[linha, coluna]) > Math.Abs(a[melhor, coluna])) melhor = linha;

            if (Math.Abs(a[melhor, coluna]) < 1e-300) return null;

            if (melhor != coluna)
                for (var j = coluna; j <= n; j++) (a[coluna, j], a[melhor, j]) = (a[melhor, j], a[coluna, j]);

            for (var linha = coluna + 1; linha < n; linha++)
            {
                var fator = a[linha, coluna] / a[coluna, coluna];
                for (var j = coluna; j <= n; j++) a[linha, j] -= fator * a[coluna, j];
            }
        }

        var x = new double[n];
        for (var i = n - 1; i >= 0; i--)
        {
            var soma = a[i, n];
            for (var j = i + 1; j < n; j++) soma -= a[i, j] * x[j];
            x[i] = soma / a[i, i];
        }

        return x;
    }

    /// <summary>
    /// A RAZAO que dita a velocidade: o segundo maior autovalor em modulo
    /// dividido pelo maior.
    ///
    /// O erro encolhe por esse fator a cada passo. Com 0,5 sao poucas dezenas de
    /// passos; com 0,999 sao dezenas de milhares; com 1, nunca.
    /// </summary>
    public static double Razao(double[] autovalores)
    {
        ArgumentNullException.ThrowIfNull(autovalores);
        if (autovalores.Length < 2) return 0;

        var modulos = autovalores.Select(Math.Abs).OrderByDescending(x => x).ToArray();
        return modulos[0] == 0 ? 0 : modulos[1] / modulos[0];
    }
}
