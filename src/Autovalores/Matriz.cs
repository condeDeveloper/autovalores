namespace Conde.Autovalores;

/// <summary>
/// Uma MATRIZ quadrada de numeros em ponto flutuante, com as poucas contas que
/// os metodos deste repositorio precisam.
///
/// Ela e pequena de proposito. Autovalor e um assunto grande e as ideias cabem
/// em matriz de ordem tres: o metodo da potencia, o deslocamento, a rotacao de
/// Jacobi e o laco QR fazem a mesma coisa em ordem tres e em ordem mil, e em
/// ordem tres da para conferir tudo contra a conta exata.
/// </summary>
public sealed class Matriz
{
    private readonly double[,] valores;

    public Matriz(double[,] valores)
    {
        ArgumentNullException.ThrowIfNull(valores);

        if (valores.GetLength(0) != valores.GetLength(1))
            throw new ArgumentException("a matriz precisa ser quadrada", nameof(valores));

        this.valores = valores;
        Ordem = valores.GetLength(0);
    }

    public int Ordem { get; }

    public double this[int i, int j] => valores[i, j];

    public static Matriz De(params double[][] linhas)
    {
        ArgumentNullException.ThrowIfNull(linhas);

        var n = linhas.Length;
        var m = new double[n, n];

        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                m[i, j] = linhas[i][j];

        return new Matriz(m);
    }

    public static Matriz Identidade(int n)
    {
        var m = new double[n, n];
        for (var i = 0; i < n; i++) m[i, i] = 1;
        return new Matriz(m);
    }

    public static Matriz Diagonal(params double[] valores)
    {
        ArgumentNullException.ThrowIfNull(valores);

        var m = new double[valores.Length, valores.Length];
        for (var i = 0; i < valores.Length; i++) m[i, i] = valores[i];

        return new Matriz(m);
    }

    public Matriz Vezes(Matriz outra)
    {
        ArgumentNullException.ThrowIfNull(outra);

        var r = new double[Ordem, Ordem];

        for (var i = 0; i < Ordem; i++)
            for (var j = 0; j < Ordem; j++)
            {
                var soma = 0.0;
                for (var k = 0; k < Ordem; k++) soma += valores[i, k] * outra[k, j];
                r[i, j] = soma;
            }

        return new Matriz(r);
    }

    public double[] Vezes(double[] vetor)
    {
        ArgumentNullException.ThrowIfNull(vetor);

        var r = new double[Ordem];

        for (var i = 0; i < Ordem; i++)
        {
            var soma = 0.0;
            for (var j = 0; j < Ordem; j++) soma += valores[i, j] * vetor[j];
            r[i] = soma;
        }

        return r;
    }

    public Matriz Menos(Matriz outra)
    {
        ArgumentNullException.ThrowIfNull(outra);

        var r = new double[Ordem, Ordem];
        for (var i = 0; i < Ordem; i++)
            for (var j = 0; j < Ordem; j++)
                r[i, j] = valores[i, j] - outra[i, j];

        return new Matriz(r);
    }

    /// <summary>A matriz menos um multiplo da identidade, que e o deslocamento.</summary>
    public Matriz MenosDiagonal(double quanto)
    {
        var r = (double[,])valores.Clone();
        for (var i = 0; i < Ordem; i++) r[i, i] -= quanto;

        return new Matriz(r);
    }

    public Matriz Transposta()
    {
        var r = new double[Ordem, Ordem];
        for (var i = 0; i < Ordem; i++)
            for (var j = 0; j < Ordem; j++)
                r[i, j] = valores[j, i];

        return new Matriz(r);
    }

    /// <summary>
    /// O TRACO: a soma da diagonal.
    ///
    /// Ele e a soma dos autovalores, sempre, e isso e uma identidade e nao uma
    /// aproximacao. E uma das duas conferencias mais baratas que existem: a
    /// outra e o determinante.
    /// </summary>
    public double Traco()
    {
        var soma = 0.0;
        for (var i = 0; i < Ordem; i++) soma += valores[i, i];

        return soma;
    }

    /// <summary>
    /// O DETERMINANTE, por eliminacao com pivotamento.
    ///
    /// Ele e o produto dos autovalores. Junto com o traco, da para conferir um
    /// conjunto de autovalores sem saber calcular nenhum deles.
    /// </summary>
    public double Determinante()
    {
        var a = (double[,])valores.Clone();
        var sinal = 1.0;

        for (var coluna = 0; coluna < Ordem; coluna++)
        {
            var melhor = coluna;
            for (var linha = coluna + 1; linha < Ordem; linha++)
                if (Math.Abs(a[linha, coluna]) > Math.Abs(a[melhor, coluna])) melhor = linha;

            if (Math.Abs(a[melhor, coluna]) < 1e-300) return 0;

            if (melhor != coluna)
            {
                for (var j = 0; j < Ordem; j++) (a[coluna, j], a[melhor, j]) = (a[melhor, j], a[coluna, j]);
                sinal = -sinal;
            }

            for (var linha = coluna + 1; linha < Ordem; linha++)
            {
                var fator = a[linha, coluna] / a[coluna, coluna];
                for (var j = coluna; j < Ordem; j++) a[linha, j] -= fator * a[coluna, j];
            }
        }

        var produto = sinal;
        for (var i = 0; i < Ordem; i++) produto *= a[i, i];

        return produto;
    }

    /// <summary>Se a matriz e igual a propria transposta, o que muda tudo.</summary>
    public bool Simetrica(double folga = 1e-12)
    {
        for (var i = 0; i < Ordem; i++)
            for (var j = i + 1; j < Ordem; j++)
                if (Math.Abs(valores[i, j] - valores[j, i]) > folga) return false;

        return true;
    }

    /// <summary>A soma dos quadrados dos elementos FORA da diagonal.</summary>
    public double ForaDaDiagonal()
    {
        var soma = 0.0;

        for (var i = 0; i < Ordem; i++)
            for (var j = 0; j < Ordem; j++)
                if (i != j) soma += valores[i, j] * valores[i, j];

        return soma;
    }

    public double[] DiagonalComo()
    {
        var d = new double[Ordem];
        for (var i = 0; i < Ordem; i++) d[i] = valores[i, i];

        return d;
    }

    public double[,] Copia() => (double[,])valores.Clone();

    public override string ToString()
    {
        var linhas = new List<string>();

        for (var i = 0; i < Ordem; i++)
        {
            var celulas = new List<string>();
            for (var j = 0; j < Ordem; j++) celulas.Add($"{valores[i, j],10:N4}");
            linhas.Add(string.Join("", celulas));
        }

        return string.Join("\n", linhas);
    }
}

/// <summary>As contas de vetor que os metodos usam.</summary>
public static class Vetor
{
    public static double Norma(double[] a)
    {
        ArgumentNullException.ThrowIfNull(a);

        var soma = 0.0;
        foreach (var x in a) soma += x * x;

        return Math.Sqrt(soma);
    }

    public static double Interno(double[] a, double[] b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var soma = 0.0;
        for (var i = 0; i < a.Length; i++) soma += a[i] * b[i];

        return soma;
    }

    public static double[] Normalizar(double[] a)
    {
        var norma = Norma(a);
        if (norma == 0) return (double[])a.Clone();

        var r = new double[a.Length];
        for (var i = 0; i < a.Length; i++) r[i] = a[i] / norma;

        return r;
    }

    public static double[] Menos(double[] a, double[] b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var r = new double[a.Length];
        for (var i = 0; i < a.Length; i++) r[i] = a[i] - b[i];

        return r;
    }

    public static double[] Vezes(double escalar, double[] a)
    {
        ArgumentNullException.ThrowIfNull(a);

        var r = new double[a.Length];
        for (var i = 0; i < a.Length; i++) r[i] = escalar * a[i];

        return r;
    }
}
