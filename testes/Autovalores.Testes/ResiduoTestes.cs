using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

public class ResiduoTestes
{
    /// <summary>
    /// O residuo de um autovalor de verdade e zero, e o de um numero qualquer
    /// nao e. E a definicao, conferida nos dois sentidos.
    /// </summary>
    [Fact]
    public void OResiduoSeparaAutovalorDeNumeroQualquer()
    {
        var diagonal = Matriz.Diagonal(5, 3, 1);

        Assert.True(Residuo.De(diagonal, 5, [1, 0, 0]) < 1e-14);
        Assert.True(Residuo.De(diagonal, 3, [0, 1, 0]) < 1e-14);
        Assert.True(Residuo.De(diagonal, 4, [1, 0, 0]) > 0.5);
        Assert.True(Residuo.De(diagonal, 5, [1, 1, 0]) > 0.5);
    }

    /// <summary>
    /// As duas identidades valem SEMPRE, inclusive em matriz com autovalor
    /// repetido e em matriz que nenhum metodo real diagonaliza.
    /// </summary>
    [Fact]
    public void AsIdentidadesValemEmTodasAsMatrizes()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var exatos = Exata.De(matriz)!;

            Assert.True(Residuo.ErroDoTraco(matriz, exatos) < 1e-10, nome);
            Assert.True(Residuo.ErroDoDeterminante(matriz, exatos) < 1e-10, nome);
        }
    }

    /// <summary>
    /// E elas PEGAM um conjunto errado: trocar um autovalor por outro numero
    /// quebra as duas na hora.
    /// </summary>
    [Fact]
    public void AsIdentidadesPegamUmConjuntoErrado()
    {
        var matriz = Casos.Simetrica();
        var errado = Exata.De(matriz)!.ToArray();
        errado[0] += 0.5;

        Assert.True(Residuo.ErroDoTraco(matriz, errado) > 0.4);
        Assert.True(Residuo.ErroDoDeterminante(matriz, errado) > 0.5);
    }

    /// <summary>
    /// A distancia entre dois conjuntos ORDENA os dois antes de comparar.
    /// Autovalor nao tem ordem natural, e comparar sem ordenar compara numeros
    /// que nao tem nada a ver.
    /// </summary>
    [Fact]
    public void ADistanciaOrdenaAntesDeComparar()
    {
        Assert.Equal(0, Residuo.Distancia([1, 2, 3], [3, 1, 2]), 12);
        Assert.Equal(1, Residuo.Distancia([1, 2, 3], [1, 2, 4]), 12);
        Assert.True(double.IsInfinity(Residuo.Distancia([1, 2], [1, 2, 3])));
    }

    /// <summary>
    /// O ACHADO: nas matrizes SIMETRICAS, uma perturbacao de tamanho e move os
    /// autovalores no maximo e. O condicionamento e perfeito, e isso e
    /// propriedade da matriz.
    /// </summary>
    [Fact]
    public void NasSimetricasAPerturbacaoNaoEhAmplificada()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            if (!matriz.Simetrica()) continue;

            var andou = Residuo.Sensibilidade(matriz, 1e-10);
            Assert.True(andou <= 1.5e-10, $"{nome}: andou {andou}");
        }
    }

    /// <summary>
    /// E no bloco de JORDAN ela e amplificada pela RAIZ: 1e-10 de perturbacao
    /// move os autovalores em 1e-5.
    ///
    /// Autovalor repetido sem base de autovetores faz o PROBLEMA ser mal
    /// condicionado, e nenhum metodo conserta, porque nao e defeito de metodo.
    /// </summary>
    [Fact]
    public void NoJordanOErroEhAmplificadoPelaRaiz()
    {
        var jordan = Casos.Jordan();

        foreach (var tamanho in new[] { 1e-4, 1e-6, 1e-8, 1e-10, 1e-12 })
        {
            var andou = Residuo.Sensibilidade(jordan, tamanho);
            Assert.Equal(Math.Sqrt(tamanho), andou, 9);
        }
    }

    [Fact]
    public void ASensibilidadeDeMatrizSemAutovalorRealNaoEhNumero()
    {
        Assert.True(double.IsNaN(Residuo.Sensibilidade(Casos.Rotacao(), 1e-10)));
    }

    /// <summary>
    /// O residuo normaliza o autovetor antes de medir: um autovetor multiplicado
    /// por dez continua sendo o mesmo autovetor.
    /// </summary>
    [Fact]
    public void OResiduoNaoDependeDoTamanhoDoAutovetor()
    {
        var diagonal = Matriz.Diagonal(5, 3, 1);

        Assert.Equal(Residuo.De(diagonal, 5, [1, 0, 0]), Residuo.De(diagonal, 5, [10, 0, 0]), 12);
        Assert.Equal(Residuo.De(diagonal, 4, [1, 0, 0]), Residuo.De(diagonal, 4, [10, 0, 0]), 12);
    }

    /// <summary>
    /// E o residuo dos autovetores que o Jacobi devolve e desprezivel em todas
    /// as matrizes simetricas.
    /// </summary>
    [Fact]
    public void OsAutovetoresDoJacobiTemResiduoDesprezivel()
    {
        foreach (var (nome, matriz) in Casos.Simetricas())
        {
            var r = Jacobi.Diagonalizar(matriz);

            for (var k = 0; k < matriz.Ordem; k++)
            {
                var v = new double[matriz.Ordem];
                for (var i = 0; i < matriz.Ordem; i++) v[i] = r.Autovetores[i, k];

                var autovalor = Vetor.Interno(v, matriz.Vezes(v));
                Assert.True(Residuo.De(matriz, autovalor, v) < 1e-9, $"{nome}, autovetor {k}");
            }
        }
    }
}
