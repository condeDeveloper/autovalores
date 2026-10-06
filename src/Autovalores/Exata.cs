namespace Conde.Autovalores;

/// <summary>
/// Os autovalores EXATOS, por formula fechada. Este e o juiz do repositorio.
///
/// Metodo iterativo de autovalor erra de um jeito silencioso: ele devolve um
/// numero que mudou pouco na ultima iteracao, e "mudou pouco" nao e "esta
/// certo". Uma matriz quase singular, ou com dois autovalores quase iguais, faz
/// a iteracao parecer convergida longe da resposta.
///
/// Em ordem dois a formula e a de Bhaskara aplicada ao polinomio caracteristico.
/// Em ordem tres e a de Cardano, que resolve a cubica e devolve as tres raizes
/// reais quando elas existem. Com esses dois casos da para julgar tudo que os
/// metodos iterativos fazem, sem pedir que ninguem acredite.
/// </summary>
public static class Exata
{
    /// <summary>
    /// Os autovalores de uma matriz DOIS por dois.
    ///
    /// O polinomio caracteristico e x^2 - traco*x + determinante, e o
    /// discriminante decide tudo: positivo da dois reais, zero da um repetido, e
    /// NEGATIVO da dois complexos conjugados, que nenhum metodo real deste
    /// repositorio acha.
    /// </summary>
    public static double[]? De2(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        if (matriz.Ordem != 2) throw new ArgumentException("esta formula so vale em ordem dois", nameof(matriz));

        var traco = matriz.Traco();
        var determinante = matriz.Determinante();
        var dentro = traco * traco - 4 * determinante;

        // Autovalor complexo existe e e comum: a rotacao de noventa graus nao
        // tem autovetor real nenhum, porque ela nao deixa direcao nenhuma no
        // lugar. Devolver nulo e dizer isso em voz alta.
        if (dentro < 0) return null;

        var raiz = Math.Sqrt(dentro);
        var um = (traco + raiz) / 2;
        var outro = (traco - raiz) / 2;

        return um >= outro ? [um, outro] : [outro, um];
    }

    /// <summary>
    /// Os autovalores de uma matriz TRES por tres, por Cardano.
    ///
    /// O polinomio caracteristico de ordem tres sempre tem pelo menos uma raiz
    /// real, e quando as tres sao reais a formula passa pelo chamado caso
    /// irredutivel, em que o caminho mais curto ate as raizes reais passa por
    /// numeros complexos. A versao trigonometrica usada aqui evita isso.
    /// </summary>
    public static double[]? De3(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        if (matriz.Ordem != 3) throw new ArgumentException("esta formula so vale em ordem tres", nameof(matriz));

        // x^3 - c2 x^2 + c1 x - c0, com c2 o traco, c1 a soma dos menores
        // principais de ordem dois e c0 o determinante.
        var c2 = matriz.Traco();
        var c0 = matriz.Determinante();

        var c1 = 0.0;
        for (var i = 0; i < 3; i++)
            for (var j = i + 1; j < 3; j++)
                c1 += matriz[i, i] * matriz[j, j] - matriz[i, j] * matriz[j, i];

        // Tirar o termo quadratico leva a y^3 + p y + q com x = y + c2/3.
        var p = c1 - c2 * c2 / 3;
        var q = -2 * c2 * c2 * c2 / 27 + c2 * c1 / 3 - c0;

        if (Math.Abs(p) < 1e-14 && Math.Abs(q) < 1e-14)
        {
            var repetido = c2 / 3;
            return [repetido, repetido, repetido];
        }

        var discriminante = -4 * p * p * p - 27 * q * q;
        if (discriminante < -1e-9) return null;

        var raizes = new double[3];
        var fator = 2 * Math.Sqrt(-p / 3);
        var dentro = Math.Clamp(3 * q / (p * fator), -1, 1);
        var angulo = Math.Acos(dentro) / 3;

        for (var k = 0; k < 3; k++)
            raizes[k] = fator * Math.Cos(angulo - 2 * Math.PI * k / 3) + c2 / 3;

        Array.Sort(raizes);
        Array.Reverse(raizes);
        return raizes;
    }

    /// <summary>Os autovalores exatos, quando a ordem permite.</summary>
    public static double[]? De(Matriz matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        return matriz.Ordem switch
        {
            1 => [matriz[0, 0]],
            2 => De2(matriz),
            3 => De3(matriz),
            _ => matriz.Simetrica() ? Jacobi.Autovalores(matriz) : null,
        };
    }

    /// <summary>
    /// O polinomio caracteristico avaliado num ponto: o determinante de
    /// A menos x vezes a identidade.
    ///
    /// Ele e zero exatamente nos autovalores, e serve para conferir uma raiz sem
    /// confiar em nenhum metodo iterativo.
    /// </summary>
    public static double Caracteristico(Matriz matriz, double x)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        return matriz.MenosDiagonal(x).Determinante();
    }
}
