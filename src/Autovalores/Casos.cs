namespace Conde.Autovalores;

/// <summary>
/// As matrizes de teste, escolhidas para separar os metodos.
///
/// Cada uma existe para quebrar uma coisa. A rotacao nao tem autovalor real
/// nenhum; a do espelho tem dois de mesmo modulo e sinais opostos, e o metodo da
/// potencia oscila nela para sempre; a de autovalores proximos faz o laco QR sem
/// deslocamento girar milhares de vezes; e a de Jordan tem autovalor repetido
/// sem base de autovetores, que e onde a sensibilidade explode.
/// </summary>
public static class Casos
{
    /// <summary>Diagonal: os autovalores estao escritos nela, sem conta nenhuma.</summary>
    public static Matriz Diagonal() => Matriz.Diagonal(5, 3, 1);

    /// <summary>
    /// Simetrica de ordem tres, com tres autovalores reais bem separados. E o
    /// caso bom, em que todo metodo funciona.
    /// </summary>
    public static Matriz Simetrica() => Matriz.De([4, 1, 1], [1, 3, 0], [1, 0, 2]);

    /// <summary>
    /// A ROTACAO de noventa graus: nenhum autovalor real.
    ///
    /// Ela nao deixa direcao nenhuma no lugar, entao nao ha autovetor real para
    /// metodo real nenhum achar. Os autovalores sao i e menos i.
    /// </summary>
    public static Matriz Rotacao() => Matriz.De([0, -1], [1, 0]);

    /// <summary>
    /// O ESPELHO: autovalores 1 e -1, de mesmo modulo.
    ///
    /// E a matriz em que o metodo da potencia nao converge nunca. As duas
    /// parcelas do vetor nunca se separam, porque nenhuma domina a outra, e o
    /// vetor oscila entre duas direcoes para sempre.
    /// </summary>
    public static Matriz Espelho() => Matriz.De([0, 1], [1, 0]);

    /// <summary>
    /// Dois autovalores quase iguais: 1 e 1,001.
    ///
    /// A razao entre eles e 0,999, entao o metodo da potencia precisa de
    /// dezenas de milhares de passos e o laco QR sem deslocamento gira milhares
    /// de vezes. Com deslocamento, sao algumas dezenas.
    /// </summary>
    public static Matriz Proximos() => Matriz.De([1, 0.0001], [0.0001, 1.001]);

    /// <summary>
    /// Um bloco de JORDAN: autovalor 1 repetido, com UM autovetor so.
    ///
    /// Ela e a matriz em que o problema, e nao o metodo, e mal condicionado: uma
    /// perturbacao de 1e-10 num canto move os autovalores em 1e-5, que e a RAIZ
    /// dela. Nenhum metodo conserta isso, porque nao e defeito de metodo.
    /// </summary>
    public static Matriz Jordan() => Matriz.De([1, 1], [0, 1]);

    /// <summary>
    /// A ARMADILHA DO VETOR DE PARTIDA: autovalores 4 e 2, e o vetor de todos
    /// iguais a um e autovetor do DOIS.
    ///
    /// As linhas somam dois, entao o vetor de uns ja e autovetor, e do autovalor
    /// que NAO e o dominante. O metodo da potencia comecando ali fica parado
    /// nele para sempre e devolve 2 dizendo que convergiu.
    ///
    /// Comecar com todos iguais a um e o palpite mais natural do mundo, e e por
    /// isso que esta matriz esta aqui.
    /// </summary>
    public static Matriz LinhasIguais() => Matriz.De([3, -1], [-1, 3]);

    /// <summary>Simetrica de ordem quatro, para Jacobi ter trabalho de verdade.</summary>
    public static Matriz Simetrica4() =>
        Matriz.De([4, 1, -2, 2], [1, 2, 0, 1], [-2, 0, 3, -2], [2, 1, -2, -1]);

    /// <summary>
    /// Quase dependente: duas colunas quase iguais.
    ///
    /// E nela que o Gram-Schmidt classico perde a ortogonalidade e o modificado
    /// segura, fazendo as mesmas contas em outra ordem.
    /// </summary>
    public static Matriz QuaseDependente()
    {
        const double perto = 1e-8;
        return Matriz.De([1, 1, 1], [perto, 0, 0], [0, perto, 0]);
    }

    public static IEnumerable<(string Nome, Matriz Matriz)> Todas()
    {
        yield return ("diagonal", Diagonal());
        yield return ("simetrica", Simetrica());
        yield return ("linhas iguais", LinhasIguais());
        yield return ("proximos", Proximos());
        yield return ("espelho", Espelho());
        yield return ("Jordan", Jordan());
        yield return ("rotacao", Rotacao());
    }

    /// <summary>As que tem todos os autovalores reais, que e onde os metodos reais valem.</summary>
    public static IEnumerable<(string Nome, Matriz Matriz)> ComAutovaloresReais() =>
        Todas().Where(c => Exata.De(c.Matriz) is not null);

    /// <summary>As simetricas, onde Jacobi vale e tudo e bem comportado.</summary>
    public static IEnumerable<(string Nome, Matriz Matriz)> Simetricas()
    {
        yield return ("diagonal", Diagonal());
        yield return ("simetrica", Simetrica());
        yield return ("proximos", Proximos());
        yield return ("espelho", Espelho());
        yield return ("simetrica de ordem 4", Simetrica4());
    }
}
