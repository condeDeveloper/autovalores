namespace Conde.Autovalores;

/// <summary>
/// As CONFERENCIAS que nao dependem de nenhum metodo.
///
/// A mais direta e a definicao: se v e autovetor de autovalor L, entao A vezes v
/// menos L vezes v e o vetor zero. O tamanho desse vetor e o RESIDUO, e ele e a
/// unica medida de "este autovalor esta certo" que nao depende de confiar em
/// quem calculou.
///
/// As outras duas sao identidades e saem de graca: a soma dos autovalores e o
/// TRACO e o produto e o DETERMINANTE. Elas nao dizem quais autovalores sao, e
/// dizem na hora se o conjunto esta errado, e custam uma varredura da diagonal.
/// </summary>
public static class Residuo
{
    /// <summary>O tamanho de A*v menos L*v, que deveria ser zero.</summary>
    public static double De(Matriz matriz, double autovalor, double[] autovetor)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        ArgumentNullException.ThrowIfNull(autovetor);

        var normalizado = Vetor.Normalizar(autovetor);
        return Vetor.Norma(Vetor.Menos(matriz.Vezes(normalizado), Vetor.Vezes(autovalor, normalizado)));
    }

    /// <summary>
    /// A soma dos autovalores menos o traco, que deveria ser zero.
    ///
    /// Esta identidade vale SEMPRE, inclusive para matriz com autovalor
    /// repetido e para matriz que nenhum metodo real diagonaliza. Ela nao prova
    /// que os autovalores estao certos; ela prova que estao errados, quando
    /// estao.
    /// </summary>
    public static double ErroDoTraco(Matriz matriz, double[] autovalores)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        ArgumentNullException.ThrowIfNull(autovalores);

        return Math.Abs(autovalores.Sum() - matriz.Traco());
    }

    /// <summary>O produto dos autovalores menos o determinante.</summary>
    public static double ErroDoDeterminante(Matriz matriz, double[] autovalores)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        ArgumentNullException.ThrowIfNull(autovalores);

        var produto = 1.0;
        foreach (var x in autovalores) produto *= x;

        return Math.Abs(produto - matriz.Determinante());
    }

    /// <summary>
    /// O erro relativo entre dois conjuntos de autovalores, ja ordenados.
    ///
    /// A ordenacao importa: autovalor nao tem ordem natural, e comparar dois
    /// conjuntos sem ordenar os dois compara numeros que nao tem nada a ver.
    /// </summary>
    public static double Distancia(double[] primeiro, double[] segundo)
    {
        ArgumentNullException.ThrowIfNull(primeiro);
        ArgumentNullException.ThrowIfNull(segundo);

        if (primeiro.Length != segundo.Length) return double.PositiveInfinity;

        var a = primeiro.OrderByDescending(x => x).ToArray();
        var b = segundo.OrderByDescending(x => x).ToArray();

        var pior = 0.0;
        for (var i = 0; i < a.Length; i++) pior = Math.Max(pior, Math.Abs(a[i] - b[i]));

        return pior;
    }

    /// <summary>
    /// O CONDICIONAMENTO do problema: quanto os autovalores andam quando a
    /// matriz e cutucada.
    ///
    /// Esse numero e sobre a MATRIZ, e nao sobre o metodo. Numa matriz
    /// simetrica, uma perturbacao de tamanho e move os autovalores no maximo e,
    /// e nao ha metodo ruim que estrague isso nem metodo bom que melhore. Numa
    /// matriz nao simetrica com autovalor repetido, a mesma perturbacao pode
    /// mover os autovalores pela RAIZ de e, que e muito maior.
    /// </summary>
    public static double Sensibilidade(Matriz matriz, double tamanho)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        var originais = Exata.De(matriz);
        if (originais is null) return double.NaN;

        var pior = 0.0;

        for (var i = 0; i < matriz.Ordem; i++)
            for (var j = 0; j < matriz.Ordem; j++)
            {
                var cutucada = matriz.Copia();
                cutucada[i, j] += tamanho;

                var depois = Exata.De(new Matriz(cutucada));
                if (depois is null) continue;

                pior = Math.Max(pior, Distancia(originais, depois));
            }

        return pior;
    }
}
