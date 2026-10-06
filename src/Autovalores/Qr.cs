namespace Conde.Autovalores;

/// <summary>
/// O ALGORITMO QR: fatorar, multiplicar ao contrario, repetir.
///
/// Ele e o metodo que o mundo usa, e a descricao dele cabe em duas linhas:
/// escreva A como Q vezes R, com Q ortogonal e R triangular superior, e troque A
/// por R vezes Q. Repita.
///
/// Nao e obvio que isso faca alguma coisa, e faz: R vezes Q e Q transposto vezes
/// A vezes Q, que e a MESMA matriz vista de outro angulo, com os mesmos
/// autovalores. E a cada volta ela fica mais triangular, ate a diagonal ser os
/// autovalores.
///
/// A fatoracao aqui e por Gram-Schmidt MODIFICADO, e a escolha tem medida: a
/// versao classica perde a ortogonalidade de Q quando as colunas sao quase
/// dependentes, e a modificada faz as mesmas contas em outra ordem e segura. A
/// medida do desvio de ortogonalidade mostra as duas lado a lado.
/// </summary>
public static class Qr
{
    public sealed record Fatoracao(Matriz Q, Matriz R);

    /// <summary>Gram-Schmidt MODIFICADO: subtrair cada projecao assim que ela e calculada.</summary>
    public static Fatoracao Fatorar(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var n = matriz.Ordem;
        var q = new double[n, n];
        var r = new double[n, n];
        var colunas = new double[n][];

        for (var j = 0; j < n; j++)
        {
            colunas[j] = new double[n];
            for (var i = 0; i < n; i++) colunas[j][i] = matriz[i, j];
        }

        for (var j = 0; j < n; j++)
        {
            var v = colunas[j];

            for (var k = 0; k < j; k++)
            {
                var qk = new double[n];
                for (var i = 0; i < n; i++) qk[i] = q[i, k];

                // A projecao usa o v JA ENCOLHIDO pelas anteriores, e nao a
                // coluna original. E so isso que separa a versao modificada da
                // classica, e e o que segura a ortogonalidade.
                r[k, j] = Vetor.Interno(qk, v);
                v = Vetor.Menos(v, Vetor.Vezes(r[k, j], qk));
            }

            var norma = Vetor.Norma(v);
            r[j, j] = norma;

            if (norma > 1e-300)
                for (var i = 0; i < n; i++) q[i, j] = v[i] / norma;
            else
                q[j, j] = 1;
        }

        return new Fatoracao(new Matriz(q), new Matriz(r));
    }

    /// <summary>A versao CLASSICA, que existe aqui so para a medida mostrar o estrago.</summary>
    public static Fatoracao FatorarClassico(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var n = matriz.Ordem;
        var q = new double[n, n];
        var r = new double[n, n];

        for (var j = 0; j < n; j++)
        {
            var original = new double[n];
            for (var i = 0; i < n; i++) original[i] = matriz[i, j];

            var v = (double[])original.Clone();

            for (var k = 0; k < j; k++)
            {
                var qk = new double[n];
                for (var i = 0; i < n; i++) qk[i] = q[i, k];

                // Aqui a projecao usa a coluna ORIGINAL, e nao a encolhida.
                r[k, j] = Vetor.Interno(qk, original);
                v = Vetor.Menos(v, Vetor.Vezes(r[k, j], qk));
            }

            var norma = Vetor.Norma(v);
            r[j, j] = norma;

            if (norma > 1e-300)
                for (var i = 0; i < n; i++) q[i, j] = v[i] / norma;
            else
                q[j, j] = 1;
        }

        return new Fatoracao(new Matriz(q), new Matriz(r));
    }

    /// <summary>
    /// Quanto Q se afastou de ser ortogonal: a maior diferenca entre Q
    /// transposto vezes Q e a identidade.
    ///
    /// Numa fatoracao perfeita isso e zero. E o numero que separa a versao
    /// classica da modificada quando as colunas sao quase dependentes.
    /// </summary>
    public static double DesvioDeOrtogonalidade(Matriz q)
    {
        ArgumentNullException.ThrowIfNull(q);

        var produto = q.Transposta().Vezes(q);
        var pior = 0.0;

        for (var i = 0; i < q.Ordem; i++)
            for (var j = 0; j < q.Ordem; j++)
                pior = Math.Max(pior, Math.Abs(produto[i, j] - (i == j ? 1 : 0)));

        return pior;
    }

    /// <summary>Quanto Q vezes R se afastou da matriz original.</summary>
    public static double ErroDaFatoracao(Matriz matriz, Fatoracao fatoracao)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        ArgumentNullException.ThrowIfNull(fatoracao);

        var produto = fatoracao.Q.Vezes(fatoracao.R);
        var pior = 0.0;

        for (var i = 0; i < matriz.Ordem; i++)
            for (var j = 0; j < matriz.Ordem; j++)
                pior = Math.Max(pior, Math.Abs(produto[i, j] - matriz[i, j]));

        return pior;
    }

    public sealed record Resultado(double[] Autovalores, int Iteracoes, bool Convergiu, Matriz Final);

    /// <summary>
    /// O laco QR SEM deslocamento.
    ///
    /// Ele funciona e e lento: a convergencia de cada elemento abaixo da
    /// diagonal e a razao entre os autovalores vizinhos, elevada ao numero de
    /// voltas. Com dois autovalores parecidos, isso e quase um, e o laco gira
    /// milhares de vezes.
    /// </summary>
    public static Resultado Iterar(Matriz matriz, double tolerancia = 1e-12, int limite = 50_000)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var atual = matriz;

        for (var passo = 1; passo <= limite; passo++)
        {
            var (q, r) = Fatorar(atual);
            atual = r.Vezes(q);

            if (AbaixoDaDiagonal(atual) < tolerancia)
                return new Resultado(Diagonal(atual), passo, true, atual);
        }

        return new Resultado(Diagonal(atual), limite, false, atual);
    }

    /// <summary>A maior entrada abaixo da diagonal, que e o quanto falta convergir.</summary>
    public static double AbaixoDaDiagonal(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var pior = 0.0;

        for (var i = 1; i < matriz.Ordem; i++)
            for (var j = 0; j < i; j++)
                pior = Math.Max(pior, Math.Abs(matriz[i, j]));

        return pior;
    }

    public static double[] Diagonal(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var d = matriz.DiagonalComo();
        Array.Sort(d);
        Array.Reverse(d);
        return d;
    }
}
