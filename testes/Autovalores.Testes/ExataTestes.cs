using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

/// <summary>
/// O juiz tambem e julgado.
///
/// A formula fechada e contra quem todos os metodos iterativos sao medidos. Se
/// ela estivesse errada, o repositorio inteiro estaria conferindo metodo contra
/// a coisa errada, e a conferencia passaria.
/// </summary>
public class ExataTestes
{
    /// <summary>
    /// Os autovalores achados ZERAM o polinomio caracteristico, que e a
    /// definicao. Isso nao usa a formula de novo: usa o determinante.
    /// </summary>
    [Fact]
    public void OsAutovaloresZeramOPolinomioCaracteristico()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
            foreach (var x in Exata.De(matriz)!)
                Assert.True(Math.Abs(Exata.Caracteristico(matriz, x)) < 1e-9,
                            $"{nome}: o polinomio em {x} deu {Exata.Caracteristico(matriz, x)}");
    }

    /// <summary>
    /// E eles respeitam as duas identidades: a soma e o traco e o produto e o
    /// determinante.
    /// </summary>
    [Fact]
    public void ASomaEhOTracoEOProdutoEhODeterminante()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var exatos = Exata.De(matriz)!;

            Assert.True(Residuo.ErroDoTraco(matriz, exatos) < 1e-10, nome);
            Assert.True(Residuo.ErroDoDeterminante(matriz, exatos) < 1e-10, nome);
        }
    }

    /// <summary>
    /// A diagonal tem os autovalores escritos nela, e mesmo assim a formula
    /// fechada nao devolve os numeros CRAVADOS.
    ///
    /// Em ordem tres ela passa por Cardano, com raiz, arco cosseno e cosseno, e
    /// o 1 da matriz diagonal volta como 0,99999999999999956. A formula e exata
    /// em matematica e nao e exata em ponto flutuante, e e por isso que a
    /// comparacao aqui tem tolerancia: pedir igualdade bit a bit seria pedir
    /// uma coisa que a formula nunca prometeu.
    /// </summary>
    [Fact]
    public void ADiagonalTemOsAutovaloresEscritosNela()
    {
        var tres = Exata.De(Matriz.Diagonal(5, 3, 1))!;

        Assert.Equal(5, tres[0], 12);
        Assert.Equal(3, tres[1], 12);
        Assert.Equal(1, tres[2], 12);

        // e em ordem dois, onde a formula e so Bhaskara, ela sai cravada
        Assert.Equal(new[] { 1.0, 1 }, Exata.De(Matriz.Identidade(2))!);
    }

    [Fact]
    public void OsCasosConhecidosDeOrdemDoisBatem()
    {
        Assert.Equal(new[] { 1.0, -1 }, Exata.De2(Casos.Espelho())!);
        Assert.Equal(new[] { 1.0, 1 }, Exata.De2(Casos.Jordan())!);
        Assert.Equal(new[] { 4.0, 2 }, Exata.De2(Casos.LinhasIguais())!);
    }

    /// <summary>
    /// A ROTACAO de noventa graus NAO tem autovalor real, e a formula diz isso
    /// em vez de inventar um numero. Ela nao deixa direcao nenhuma no lugar.
    /// </summary>
    [Fact]
    public void ARotacaoNaoTemAutovalorReal()
    {
        Assert.Null(Exata.De2(Casos.Rotacao()));
        Assert.Null(Exata.De(Casos.Rotacao()));
    }

    /// <summary>
    /// Em ordem tres a formula e a de Cardano, e ela acerta os tres autovalores
    /// de uma simetrica, conferidos contra Jacobi.
    /// </summary>
    [Fact]
    public void EmOrdemTresCardanoBateComJacobi()
    {
        var simetrica = Casos.Simetrica();

        Assert.Equal(0, Residuo.Distancia(Exata.De3(simetrica)!, Jacobi.Autovalores(simetrica)), 9);
    }

    [Fact]
    public void EBateEmMatrizesSimetricasSorteadas()
    {
        var estado = 7UL;

        double Proximo()
        {
            estado = estado * 6364136223846793005UL + 1442695040888963407UL;
            return (int)(estado >> 33) % 21 - 10;
        }

        for (var caso = 0; caso < 40; caso++)
        {
            var m = new double[3, 3];
            for (var i = 0; i < 3; i++)
                for (var j = i; j < 3; j++)
                {
                    m[i, j] = Proximo();
                    m[j, i] = m[i, j];
                }

            var matriz = new Matriz(m);
            var exatos = Exata.De3(matriz);
            if (exatos is null) continue;

            Assert.True(Residuo.Distancia(exatos, Jacobi.Autovalores(matriz)) < 1e-8);
        }
    }

    [Fact]
    public void OPolinomioCaracteristicoEhODeterminanteDeslocado()
    {
        var matriz = Casos.Simetrica();

        Assert.Equal(matriz.Determinante(), Exata.Caracteristico(matriz, 0), 9);
        Assert.Equal(matriz.MenosDiagonal(2).Determinante(), Exata.Caracteristico(matriz, 2), 9);
    }

    [Fact]
    public void AsFormulasRecusamAOrdemErrada()
    {
        Assert.Throws<ArgumentException>(() => Exata.De2(Casos.Simetrica()));
        Assert.Throws<ArgumentException>(() => Exata.De3(Casos.Espelho()));
    }

    [Fact]
    public void OrdemUmEhOProprioNumero()
    {
        Assert.Equal(new[] { 7.0 }, Exata.De(Matriz.Diagonal(7))!);
    }

    /// <summary>
    /// A matriz com os tres autovalores iguais cai no caso degenerado da formula
    /// de Cardano, e ela devolve o numero repetido em vez de dividir por zero.
    /// </summary>
    [Fact]
    public void TresAutovaloresIguaisNaoQuebramCardano()
    {
        var tres = Matriz.Diagonal(2, 2, 2);
        Assert.Equal(new[] { 2.0, 2, 2 }, Exata.De3(tres)!);
    }
}
