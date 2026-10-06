using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

/// <summary>
/// Cada matriz de teste existe para quebrar uma coisa, e aqui cada uma prova que
/// quebra mesmo aquilo. Um caso de teste que deixou de ser o que dizia ser nao
/// testa nada e passa.
/// </summary>
public class CasosTestes
{
    [Fact]
    public void ARotacaoNaoTemMesmoAutovalorReal()
    {
        Assert.Null(Exata.De(Casos.Rotacao()));
        Assert.Equal(0, Casos.Rotacao().Traco(), 12);
        Assert.Equal(1, Casos.Rotacao().Determinante(), 12);
    }

    /// <summary>
    /// O ESPELHO tem dois autovalores de MESMO MODULO e sinais opostos, que e a
    /// condicao exata em que o metodo da potencia nao separa as parcelas.
    /// </summary>
    [Fact]
    public void OEspelhoTemDoisDeMesmoModulo()
    {
        var exatos = Exata.De(Casos.Espelho())!;

        Assert.Equal(2, exatos.Length);
        Assert.Equal(Math.Abs(exatos[0]), Math.Abs(exatos[1]), 12);
        Assert.NotEqual(exatos[0], exatos[1], 6);
        Assert.Equal(1, Potencia.Razao(exatos), 12);
    }

    /// <summary>
    /// A de autovalores PROXIMOS tem razao quase um, que e o que faz o laco sem
    /// deslocamento girar milhares de vezes.
    /// </summary>
    [Fact]
    public void AProximosTemRazaoQuaseUm()
    {
        var razao = Potencia.Razao(Exata.De(Casos.Proximos())!);

        Assert.InRange(razao, 0.99, 0.9999);
        Assert.True(Deslocamento.Comparar(Casos.Proximos()).Sem > 1000);
    }

    /// <summary>
    /// O bloco de JORDAN tem autovalor repetido com UM autovetor so, que e o que
    /// faz o problema ser mal condicionado.
    /// </summary>
    [Fact]
    public void OJordanTemAutovalorRepetidoComUmAutovetorSo()
    {
        var jordan = Casos.Jordan();
        var exatos = Exata.De(jordan)!;

        Assert.Equal(exatos[0], exatos[1], 12);
        Assert.False(jordan.Simetrica());

        // (1, 0) e autovetor; (0, 1) nao e
        Assert.True(Residuo.De(jordan, 1, [1, 0]) < 1e-14);
        Assert.True(Residuo.De(jordan, 1, [0, 1]) > 0.5);
    }

    /// <summary>
    /// A da ARMADILHA tem o vetor de todos iguais a um como autovetor do
    /// autovalor que NAO e o dominante.
    /// </summary>
    [Fact]
    public void AArmadilhaTemOVetorDeUnsComoAutovetorDoMenor()
    {
        var matriz = Casos.LinhasIguais();
        var exatos = Exata.De(matriz)!;

        Assert.Equal(4, exatos[0], 12);
        Assert.Equal(2, exatos[1], 12);
        Assert.True(Residuo.De(matriz, 2, [1, 1]) < 1e-14);
    }

    /// <summary>
    /// A QUASE DEPENDENTE tem colunas quase iguais, que e onde o Gram-Schmidt
    /// classico perde a ortogonalidade.
    /// </summary>
    [Fact]
    public void AQuaseDependenteEhMesmoQuaseDependente()
    {
        var matriz = Casos.QuaseDependente();

        Assert.True(Math.Abs(matriz.Determinante()) < 1e-14);
        Assert.True(Qr.DesvioDeOrtogonalidade(Qr.FatorarClassico(matriz).Q) > 0.1);
    }

    [Fact]
    public void AsSimetricasSaoMesmoSimetricas()
    {
        foreach (var (nome, matriz) in Casos.Simetricas())
            Assert.True(matriz.Simetrica(), nome);
    }

    [Fact]
    public void AsComAutovaloresReaisTemMesmoAutovaloresReais()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
            Assert.NotNull(Exata.De(matriz));

        Assert.DoesNotContain(Casos.ComAutovaloresReais(), c => c.Nome == "rotacao");
    }

    [Fact]
    public void TodasTemNomeEOrdemValida()
    {
        foreach (var (nome, matriz) in Casos.Todas())
        {
            Assert.False(string.IsNullOrWhiteSpace(nome));
            Assert.InRange(matriz.Ordem, 2, 4);
        }
    }

    [Fact]
    public void ADiagonalTemOsAutovaloresEscritosNela()
    {
        var diagonal = Casos.Diagonal();

        Assert.Equal(0, diagonal.ForaDaDiagonal(), 12);
        Assert.Equal(new[] { 5.0, 3, 1 }, diagonal.DiagonalComo());
    }
}
