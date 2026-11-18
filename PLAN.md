# Pamella · Plano

Especificação central do Pamella, ainda em esboço. O que for decidido aqui vira proposta no
`kanban/` e, depois de aprovado, vai para `pamella/ARCHITECTURE.md`. Nada deste plano está
implementado no Pamella.

**Como ler os exemplos e as afirmações:**
- **[comprovado]:** rodou nos testes de `exploracao/`, sobre os pacotes do NuGet, sem mudar as libs:
  - `syntax-test`: Blindness 4.0, com um `ViewNode` simulado e sem GPU;
  - `shader-wrapper-test`: Radiance 4.0, com backend falso, conferindo o GLSL gerado;
- **[conferido]:** lido no código das libs, sem teste próprio;
- **[desejado]:** é a intenção, e depende de código do Pamella que ainda não existe;
- **(validar):** ponto de risco que precisa de teste antes de virar contrato.

---

## 1. Onde estamos

| lib | versão | o que o Pamella usa |
|---|---|---|
| Radiance | 4.0.0 | janela e seus eventos, `OnDemand`, `render(...)` como `DynamicObject`, análise simbólica do delegate (fake invoke), geração de GLSL, currying e `skip`, instâncias por buffer, scissor, depth, texto |
| Blindness | 4.0.0 | concretas geradas a partir de interfaces, `[ConcreteBase]`, `Deps`, boxes, `Bind`, `On`, hot reload por recompilação |

**O Pamella:** o repositório tem o esqueleto de um desenho antigo e não compila. As 15 propostas
(Pamella-001 a 015) estão abertas e serão reescritas depois que este plano assentar.

**Responsabilidades:**

| lib | responsabilidade |
|---|---|
| **Blindness** | implementação das interfaces, armazenamento das propriedades, binding e reatividade |
| **Radiance** | shaders, fake invoke, GLSL, objetos dinâmicos, currying, geometria e execução gráfica |
| **Pamella** | ciclo de vida das views, funções construtoras, contexto espacial, composição, eventos, invalidação e integração das duas libs |

---

## 2. Princípios

1. **Árvore de nós, não de caixas.** Uma `View` é uma unidade de composição, posição e desenho, no
   estilo dos nós do Godot. Ela não é uma `div`.
2. **Responsabilidades separadas no componente:** dados nas propriedades, comportamento em
   `Behaviour`, carregamento em `Load`, shaders em `Shaders`, recursos visuais em `Design`, desenho
   em `Paint` e tempo em `Tick`.
3. **Desenho explícito.** O que aparece é o que o `Paint` chama, na ordem em que chama.
4. **Invalidação explícita.** Nada é redesenhado sem `Invalidate()` ou `[InvalidateOnChange]`.
5. **Binding, invalidação e `Paint` são independentes:** o binding decide como os valores mudam, a
   invalidação decide quando redesenhar, e o `Paint` decide como desenhar.
6. **O dev controla a geometria, o Pamella controla o deslocamento.** O polígono é sempre explícito,
   e `(x, y, z)` vêm do contexto.
7. **Não duplicar as libs.** Fake invoke, GLSL, boxes e binding continuam nelas.
8. **Um draw por shader, sempre que der.** Chamadas compatíveis são juntadas em instâncias.
9. **Tipado onde ajuda, dinâmico onde simplifica.** Componentes são tipados, e recursos gráficos são
   `dynamic`.
10. **Parado não gasta.** Sem invalidação, o `Paint` não roda.
11. **Hot reload sem promessa falsa** (seção 11).
12. **Três níveis de escrita, que se misturam:** shader para desempenho máximo, `canvas` com
    `Graphics` para simplicidade, e componentes prontos para produtividade (seção 3.1).
13. **Estado em propriedades.** A tela deriva delas por `bind` e pelo `Paint`.
14. **C# puro.** Sem linguagem de marcação nem arquivo à parte para a interface.
15. **`await` natural:** a continuação volta para o thread da UI.
16. **Abrir é uma linha:** `App.Run<MinhaTela>()`.

---

## 3. A forma de um componente

```cs
public interface MyButton : View
{
    // Dados
    [InvalidateOnChange] string Text { get; set; }
    [InvalidateOnChange, Default(50)] int Size { get; set; }
    [InvalidateOnChange] Vec4 Fill { get; set; }
    int Clicks { get; set; }
    Action OnClick { get; set; }
    Catalog Catalog { get; set; }

    void Deps(Catalog Catalog);

    // Recursos
    dynamic Background { get; set; }
    dynamic Badge { get; set; }
    dynamic Caption { get; set; }

    void Shaders()
    {
        Background = shader((vec4 backgroundColor) =>
        {
            color = backgroundColor;
            fill();
        });
    }

    void Design()
    {
        Badge = canvas((Graphics g, string value) => g.DrawString(value, Theme.Small, Theme.Text, 0, 0));
        Caption = view(ctx => Label().Bind(l => l.Text == Text));
    }

    // Comportamento
    void Behaviour()
    {
        OnMouseDown += button => OnClick?.Invoke();
        bind(() => Text == Catalog.Title);
    }

    async Task Load() => Catalog.Items = await Catalog.Fetch();

    void Tick(float dt) { }

    void Unload() { }

    // Desenho
    void Paint()
    {
        Clip(0, 0, Size * 2, Size);
        Background(Rect(Size * 2, Size), Fill);   // mais atrás
        Caption();
        Badge("novo");                            // mais na frente
    }

    Area Area => new(0, 0, Size * 2, Size);
}

// Uso, em qualquer arquivo do projeto
Panel(controls: [
    MyButton(text: "Confirmar"),
    MyButton(text: "Cancelar", size: 100),
]);
```

**[comprovado]** em `syntax-test`:
- a função construtora com o mesmo nome da interface, no **mesmo namespace** e dentro de outra
  interface;
- `Shaders` uma vez por tipo, inclusive antes de qualquer montagem;
- `Design` por instância, ligado à instância certa;
- `Behaviour` antes do `Load`, `async Task Load()`, `Unload`, `bind` e `OnKeyDown +=` contextuais;
- `[InvalidateOnChange]` com setter e com `bind`.

**[desejado]:** `Panel`, `Label`, `Rect`, `Clip`, `canvas`, `view`, `Graphics` e o desenho na GPU.

### 3.1 Três níveis de escrita

| nível | como | para quê | custo |
|---|---|---|---|
| **1 · Shader** | `shader(...)` em `Shaders`, chamado no `Paint` | desempenho máximo e efeitos | entra em lote, sem código de CPU além da gravação |
| **2 · Canvas** | `canvas(...)` com `Graphics`, no estilo do WinForms | simplicidade e liberdade | também entra em lote, mas o delegate roda no CPU a cada repintura |
| **3 · Prontos** | `Button`, `Label`, `TextBox`, `CheckBox`, `ListView`, `Column`, `Row`, `Grid`... | produtividade | o dev só configura |

Os três se misturam no mesmo `Paint`. Quando um efeito de `canvas` se repete muito na tela, ele vira
um shader.

### 3.2 Composição: decidida componente a componente

Não há regra fixa de "um tipo, um shader". O lote segue o shader, então montar um componente é
escolher quais shaders ele usa:
- **componentes base, compartilhados:** texto, painel (retângulo arredondado com fundo, borda e
  sombra), imagem e ícone. O que é feito deles entra nos mesmos lotes;
- **composição:** um `Button` pode ser um painel com um texto dentro. Não cria shader novo e divide
  o lote com os outros painéis e textos da tela;
- **do zero:** um `Button` pode ter `Shaders` próprio, com estados e efeitos que um painel não tem.
  Ganha um lote só dele;
- **por expressividade:** um componente pode trocar o shader base por um próprio, pagando um draw a
  mais.

A escolha de cada componente pronto é feita na proposta dele, olhando custo e expressividade. O dev
de app tem as mesmas opções.

### 3.3 Componente de texto

Haverá com certeza um componente de texto, que centraliza a escrita e é usado pelos outros
(`Button`, `TextBox`, `Label`...).
- **Objetivo:** um quadrado por glifo, com todos os glifos em instâncias de um mesmo draw, levando
  cor, posição e glifo. O custo deixa de crescer com o número de textos. Depende de um atlas (I6).
- **Texto expressivo:** um título com gradiente, contorno animado ou distorção pode ter shader
  próprio, e paga um draw a mais.

---

## 4. Árvore de Views e coordenadas

### 4.1 A interface `View`

```cs
[ConcreteBase(typeof(ViewNode))]
public interface View : INode
{
    [InvalidateOnChange] float LocalX { get; set; }
    [InvalidateOnChange] float LocalY { get; set; }

    float X => /* implementação padrão */;
    float Y => /* implementação padrão */;
    View Parent => /* implementação padrão, lê o ViewNode */;

    void Invalidate();
}
```

- `LocalX` e `LocalY` são a posição relativa ao pai.
- `X` e `Y` são a posição na janela, só leitura, sem gerador: têm implementação padrão.
- `Invalidate()` é abstrato na interface e vem do `ViewNode`, a base concreta. **[comprovado]**
- A `View` não tem tamanho (D21).

### 4.2 Posição efetiva

```
X = Parent.X + deslocamento de conteúdo do pai + LocalX
```

O deslocamento é declarado pelo container no `Paint`, com `Offset(dx, dy)` (decisão C22), e fica
guardado no `ViewNode`: não é propriedade da `View`.

- **Por que efetivo:** é o valor que o hit-test, um tooltip ou um menu ancorado precisam.
- **Sem rolagem,** coincide com a soma simples `Parent.X + LocalX`.
- **Custo:** ler `X` sobe a árvore até a raiz. O `ViewNode` pode guardar o valor e descartá-lo quando
  um ancestral se move, se a medição pedir.

### 4.3 Eixos e profundidade do Radiance

**[conferido]** e **[comprovado]** no GLSL gerado:
- a origem é o canto **inferior esquerdo**, com `y` para cima. O mouse já chega assim
  (`Height - e.Y`), e o scissor usa o mesmo canto;
- `z` vai de 0 a 1000 e vira `0.99999 - 2z / 1001`. Com `DepthFunc(Lequal)`, **`z` maior fica na
  frente**;
- os polígonos prontos são **centrados na origem**, e `Polygons.Square` tem lado 1.

O eixo `y` da UI fica para a decisão C34.

### 4.4 `Area` e `Measure`

```cs
public record struct Area(float X, float Y, float Width, float Height);
```

`Measure(view)`, em `Pamella.Utils`, devolve a `Area` declarada pelo componente ou, se não houver,
o retângulo que envolve o que o `Paint` dele gravou, filhos incluídos (decisão C30). `Measure(View1)`
mede a raiz de um recurso `view(...)`.

- **Para que serve:** layout do pai, hit-test e recorte.
- **A medida automática exige a view gravada.** Se ainda não foi, `Measure` grava antes. Gravar não
  desenha, só monta a lista de pintura. **(validar o custo e os ciclos: um `Paint` que depende do
  próprio `Measure`.)**
- **Com argumento `dynamic`,** `.Width` e `.Height` funcionam sem cast. O que o C# não aceita é
  desconstruir o resultado (`var (w, h) = ...`, erro CS8133). **[comprovado]**

### 4.5 Containers

Um container recebe filhos, escreve `LocalX` e `LocalY` deles a partir de `Measure` e, no `Paint`,
define recorte e deslocamento e chama `Paint(Controls)`. Nada disso está na `View`: é código do
container, visível no `Paint` dele. O layout dos prontos segue a Pamella-005.

Um container com rolagem inteiro **[desejado]**:

```cs
public interface ScrollPanel : View
{
    [InvalidateOnChange] float Width { get; set; }
    [InvalidateOnChange] float Height { get; set; }
    [InvalidateOnChange] float ScrollY { get; set; }
    View[] Controls { get; set; }

    void Paint()
    {
        Clip(0, 0, Width, Height);
        Offset(0, -ScrollY);
        Paint(Controls);
    }

    Area Area => new(0, 0, Width, Height);
}
```

---

## 5. Funções construtoras geradas

### 5.1 O que é gerado

Para cada interface de componente, **sem passo manual**:

```cs
// gerado
global using static Pamella.Generated.Components;

namespace Pamella.Generated;

internal static partial class Components
{
    public static global::App.MyButton MyButton(
        string text = null,
        int size = 50,
        global::Radiance.Vec4? fill = null,
        int? clicks = null,
        global::System.Action click = null)
    {
        var view = global::Pamella.ViewFactory.Create<global::App.MyButton>();
        if (text is not null) view.Text = text;
        view.Size = size;
        if (fill is not null) view.Fill = fill.Value;
        if (clicks is not null) view.Clicks = clicks.Value;
        if (click is not null) view.OnClick = click;
        return view;
    }
}
```

### 5.2 Regras dos parâmetros

| propriedade | parâmetro |
|---|---|
| abstrata com `set` | um parâmetro opcional, em camelCase, escapado se for palavra reservada |
| `[Default(constante)]` | o valor aparece na assinatura e no IntelliSense |
| sem `[Default]`, tipo por referência | `= null`, e não é escrita se ausente |
| sem `[Default]`, tipo de valor | `T? = null`, e não é escrita se ausente |
| `Action` ou delegate com prefixo `On` | perde o `On` (`OnClick` → `click`) |
| `LocalX`, `LocalY` | `x`, `y` |
| `dynamic` | fica de fora: é recurso |
| parâmetro de `Deps` | fica de fora: é injetado |

Um valor que não é constante em C# (`Vec4`, lista) não cabe em `[Default]`: o componente o define
no `Design` ou no `Load`, ou aceita o padrão do tipo (decisão C38).

### 5.3 Pipeline

- **Gerador próprio do Pamella**, `IIncrementalGenerator`, empacotado em `analyzers/dotnet/cs`. Ele
  roda ao lado do gerador do Blindness, sem modificá-lo (D6). A IDE roda os dois enquanto o dev
  digita, então a função existe sem compilar nem executar. **(validar no VS e no C# Dev Kit.)**
- **Modelo equatável:** a etapa de transformação guarda strings e listas imutáveis, e não símbolos,
  para o cache incremental funcionar.
- **A concreta é `partial` [conferido]:** o gerador do Blindness emite `partial class XConcrete`, e
  o do Pamella acrescenta membros à mesma classe. **[comprovado]** com um `partial` escrito à mão.

### 5.4 Conflitos e referências entre projetos

- **Tipo e função com o mesmo nome** convivem, inclusive no mesmo namespace. **[comprovado]**
- **Entre projetos:** a classe é `internal` em cada assembly, com o nome fixo que você pediu. Cada
  projeto gera também as funções dos componentes públicos dos assemblies referenciados, incluindo
  os prontos do Pamella. Assim não há colisão de `Pamella.Generated.Components`. **(validar o custo
  de varrer as referências.)**
- **Dois componentes com o mesmo nome** em namespaces diferentes: o gerador não emite nenhum dos
  dois e dá um diagnóstico, resolvido com `[Constructor("OutroNome")]`.
- **Não geram função:** interfaces genéricas ou aninhadas (o Blindness também as ignora) e as
  marcadas com `[Ignore]`. Por isso um `ListView` recebe o tipo do item por um parâmetro ou método
  genérico, e não é ele mesmo genérico.

### 5.5 Instanciar, associar e desenhar

| momento | o que acontece |
|---|---|
| **instanciar** | `Button(...)` cria o nó, injeta as dependências e aplica os argumentos. Ainda não há pai. |
| **associar** | Entregar a view a um pai (`controls: [...]`, ou a raiz de um `view(...)`) define o `Parent` e a monta (seção 9). |
| **desenhar** | A view só aparece quando o `Paint` do pai a pinta (`Paint(Controls)` ou `View1()`). |

As funções devolvem o tipo do componente, então o editor mostra parâmetros e tipos, e dá para
encadear `.Bind(b => b.Enabled == !Busy)`.

### 5.6 Geradores e `DynamicObject`: quem faz o quê

| peça | quem faz | como |
|---|---|---|
| propriedades (inclusive `dynamic`) e `Deps` | gerador do Blindness, como está | classe `<Nome>Concrete` |
| funções construtoras, `WireInvalidation` | gerador do Pamella | `Components` e parte `partial` da concreta |
| `X`, `Y`, `Parent` | ninguém | implementação padrão na `View` |
| `Shaders`, `Design`, `Behaviour`, `Load`, `Unload`, `Paint`, `Tick`, `Area` | ninguém | reflexão uma vez por tipo, no `ViewNode` |
| wrappers de shader | em aberto | C31 |

**Onde entra o `dynamic`:** só em `shader`, `canvas` e `view`, que são `DynamicObject` como o `Render`
do Radiance. Propriedades, funções construtoras, `bind`, `Measure` e eventos são tipados.

**Custo:** a primeira chamada de cada ponto do código paga a montagem do binder do `dynamic`.
**(medir.)** Quem quiser evitar declara a propriedade com o tipo do recurso (`Canvas`, `Subview`) e
usa métodos tipados.

---

## 6. Shaders

### 6.1 O contrato

```cs
dynamic Background { get; set; }

void Shaders()
{
    Background = shader((vec4 backgroundColor) =>
    {
        color = backgroundColor;
        fill();
    });
}

void Paint()
{
    Background(Polygons.Circle, blue);
    Background(myCustomPolygon, green);
}
```

- `shader(...)` fica em `Pamella.Utils`, e `render(...)` continua sendo o do Radiance.
- O corpo é a DSL normal do Radiance. Fake invoke, `ShaderContext`, geração de GLSL e `DynamicObject`
  não são reimplementados.
- O primeiro argumento é sempre o polígono, fora da assinatura declarada.
- `(x, y, z)` não aparecem nem na declaração nem na chamada.

### 6.2 Definição e aplicação

| objeto | o que guarda | quantos |
|---|---|---|
| **definição** (`ShaderDefinition`) | delegate original, wrapper, `Render` do Radiance, programas compilados | um por propriedade e interface que a declara |
| **aplicação** (`ShaderApplication`) | polígono e argumentos públicos já fixados | um por aplicação parcial |

Os dois são `DynamicObject`. A propriedade `dynamic` recebe a aplicação vazia.

### 6.3 O que a chamada faz

| onde a chamada acontece | efeito |
|---|---|
| dentro de outro shader (fake invoke) | chama o `Render` original, sem `(x, y, z)`: a composição do Radiance continua igual |
| no `Paint`, com todos os argumentos | grava uma instância na lista de pintura |
| com argumentos faltando, ou fora do `Paint` | devolve uma nova aplicação |

```cs
var redBackground = Background(skip, red);
redBackground(Polygons.Square);
```

- `skip` e os índices seguem o Radiance: o polígono é o índice 0, cada componente de vetor ocupa um
  índice, e `(x, y, z)` não contam.
- Uma aplicação incompleta dentro do `Paint` não desenha nada. Em Debug, o Pamella avisa ao fim do
  `Paint` sobre as que não foram usadas.

**Por que a aplicação não usa o currying do `Render`.** O `Render.Curry` cria um `Render` novo, com
cache vazio. **[comprovado]:** a mesma função, chamada direto e por dois currys, compilou **3
programas com GLSL idêntico**. Com um curry por instância, cada botão compilaria um shader.

Por isso a aplicação guarda os argumentos e chama a definição com tudo preenchido. O custo é repetir
no Pamella a regra de preencher os índices, de poucas linhas. Fazer o curry do Radiance compartilhar
o cache seria uma proposta, e eliminaria essa cópia (seção 14).

### 6.4 Wrapper: como `(x, y, z)` entram

O wrapper é uma função com a assinatura pública mais os três valores, e invoca o delegate original
durante o fake invoke, sem converter o corpo em Expression Tree:

```cs
(vec4 a1, val pmX, val pmY, val pmZ) =>
{
    // abre o escopo espacial
    original(a1);
}
```

- **`(x, y, z)` vão no fim.** O fluxo lógico é o da sua proposta, e essa ordem mantém os índices
  públicos iguais aos do Radiance. **[comprovado]**
- **Nomes próprios** (`a1`, `pmX`) evitam colisão com os do dev e com palavras reservadas do GLSL.
- **Compilação:** uma definição gera um programa por primitiva desenhada (`fill` e `draw` são dois)
  e por forma de argumento (uniform ou buffer). **[comprovado]**

### 6.5 Onde o deslocamento é aplicado

**Fazer `move` antes do corpo está errado.** **[comprovado]:** com `zoom(f)` no shader, o GLSL ficou
`f * (pos + offset)`, e o zoom escalou também a posição do componente. O deslocamento tem de ser a
última transformação, e o `fill()` do Radiance fixa `pos` no momento em que é chamado.

| opção | como | resultado |
|---|---|---|
| **gerador decorado (rec)** | o Pamella embrulha o `ICodeGenerator` em `ImplementationConfig` e soma o offset do escopo à posição final | **[comprovado]:** `(f * pos) + offset`, com `fill()` e `draw()` originais |
| `fill` próprio | `View` ou `Pamella.Utils` traz outro `fill` | **[comprovado]** no resultado, mas duplica cada primitiva e pode conflitar com o `using static` do Radiance |
| `move` antes do corpo | wrapper simples | transformações do dev passam a agir em coordenadas de janela |

Com o gerador decorado, `pos` no shader é sempre local à view, e `rotate` e `zoom` agem em torno da
origem dela (decisão C32).

### 6.6 Geração do wrapper: **decisão em aberto (C31)**

Os resultados abaixo informam a decisão, sem fechá-la. O Pamella isola a escolha numa interface:

```cs
interface IShaderWrapperFactory { Delegate Wrap(Delegate original); }
```

| critério | `Expression.Compile()` | Source Generator | genérico por aridade |
|---|---|---|---|
| como é | árvore montada em execução | wrapper gerado por assinatura | um método genérico por aridade, escrito uma vez e fechado com `MakeGenericMethod` |
| funciona no Radiance 4.0 | **não, hoje** | sim na forma (lambda comum) **(validar o gerador)** | **sim [comprovado]** |
| assinatura vista em execução | sim | não diretamente | sim, até a aridade escrita |
| custo de criação | 21,6 ms no primeiro, com JIT **[comprovado]** | em compilação | 0,385 ms no primeiro, 0,86 µs nos seguintes **[comprovado]** |
| diagnóstico antecipado | limitado | o mais completo | limitado |
| IntelliSense | indireto | direto nas APIs geradas | as sobrecargas de `shader` já dão tipos |
| hot reload | sem exigência extra | exige o gerador na recompilação | sem exigência extra |

- **O bloqueio do `Expression.Compile()` [comprovado]:** o delegate compilado expõe um primeiro
  parâmetro `Closure`, e os demais vêm sem nome. O `Render` lê `Method.GetParameters()` e falha com
  `UnhandleableArgumentsException`. Para essa opção valer, o Radiance precisaria ignorar o `Closure`
  e aceitar nomes ausentes, ou o wrapper teria de sair por `AssemblyBuilder`.
- **AOT pesa pouco na escolha:** `dynamic` e `DynamicInvoke` já estão no caminho do Radiance.
  **(validar.)**
- **Híbrido possível:** genérico por aridade como base, e gerador para diagnósticos ou assinaturas
  especiais.

**Experimentos para decidir:**
1. tempo de abertura com 10, 100 e 500 definições em cada estratégia;
2. custo do fake invoke com cada wrapper, que usa `DynamicInvoke` no Radiance;
3. complexidade do gerador para achar as lambdas de `shader(...)` em `Shaders`;
4. comportamento nos dois hot reloads;
5. publicação com trimming e AOT, para confirmar o que de fato é possível.

### 6.7 Armazenamento e ciclo dos shaders

- **Tabela global:** `(interface que declara, propriedade) → ShaderDefinition`. Um shader herdado é
  o mesmo para todos os derivados, e divide o lote com eles.
- **Primeira inicialização:** na abertura, numa varredura dos tipos, ou na primeira instância. A
  `Shaders` roda numa instância descartável, só com o construtor, sem `Deps` nem `Load`.
  **[comprovado]**
- **A lambda não pode capturar a instância.** O Pamella confere `delegate.Target` e dá erro claro.
- **Cada instância** recebe a aplicação vazia na propriedade `dynamic`. A `Shaders` não roda de novo
  em `Paint`, em invalidação nem em reconstrução da view. **[comprovado]**
- **Compilação do GLSL:** no primeiro desenho, como hoje. O `Render.PreLoad` do Radiance não está
  implementado (Radiance-017). Para pré-carregar sem mudar a lib, a alternativa é desenhar uma vez
  com scissor vazio num primeiro frame. **(validar.)**
- **Descarte:** ao fechar o app, ou quando o hot reload troca a definição.

**Modos de pré-carregamento, para quando a compilação antecipada entrar:**

| modo | quando compila | bom para |
|---|---|---|
| sob demanda | no primeiro desenho | componentes raros |
| na abertura | antes de a janela aparecer | o que a primeira tela usa |
| ocioso | nos frames sem trabalho depois da abertura | o resto do app |
| cache em disco | binário do programa salvo entre execuções (`glGetProgramBinary`) | abrir rápido da segunda vez em diante |

O modo pode ser escolhido por um atributo no componente ou por `App.Preload<T>()`.

---

## 7. `Paint` e o modelo de renderização

### 7.1 O contrato

- **Ordem:** o que é chamado antes fica atrás.
- **Coordenadas locais (C21):** `(0, 0)` é a posição da view.
- **Operações do contexto,** em `Pamella.Utils`:

| operação | efeito |
|---|---|
| `Clip(x, y, w, h)` ou `Clip(area)` | scissor na janela, intersectado com o do pai, até o fim do `Paint` |
| `Offset(dx, dy)` | desloca os filhos pintados depois |
| `Paint(view)`, `Paint(views)` | pinta filhos |
| `Rect(w, h)` | polígono com canto em `(0, 0)`, guardado em cache por tamanho |

Conteúdo condicional é escrito no próprio `Paint`:

```cs
void Paint()
{
    Background(Rect(Width, Height), Fill);
    if (Open)
        Menu();
}
```

### 7.2 Do `Paint` à tela

```
Invalidate()
  → marca a view e pede um frame (Window.Invalidate)
OnFrame do Radiance
  → Tick das views que têm Tick
  → Paint de cada view marcada: regrava a lista de pintura dela
OnRender do Radiance
  → junta as listas e executa os draws
```

- O `Paint` roda **fora** do `OnRender`, e só grava. Cada frame repete as listas, sem rodar `Paint`.
- Pintar um filho grava só a marca "filhos aqui". O filho tem a própria lista.

### 7.3 O contexto espacial

| valor | de onde vem |
|---|---|
| `x`, `y` | `X` e `Y` da view mais os `Offset` em vigor |
| `z` | posição na ordem de pintura, dentro do intervalo de profundidade da view |
| polígono | do dev, sem alteração |
| transformação do shader | do dev, em coordenadas locais, antes do deslocamento |
| scissor | do `Clip`, já em pixels da janela |

- **Intervalos de profundidade:** cada view recebe uma faixa de `z`, dividida entre as próprias
  chamadas e os filhos, dentro da faixa da camada.
- **Mover não repinta:** mudar `LocalX` invalida a view, mas o Pamella percebe que só a posição mudou
  e regrava `x` e `y` das instâncias dela e dos descendentes, sem rodar nenhum `Paint`.

### 7.4 Lote

Chamadas entram no mesmo draw quando têm a mesma **definição**, o mesmo **polígono** (mesma
referência) e o mesmo **scissor**. **[comprovado]:** mil instâncias, com `square * 1000` e `x`, `y`,
`z` em buffers, saíram em **1 draw e 1 programa**.

**Consequências:**
- polígonos diferentes separam lotes. `Rect(w, h)` reaproveita o objeto por tamanho, mas tamanhos
  diferentes ainda são draws diferentes (decisão C33);
- cada `Clip` diferente separa lotes, porque o scissor é estado da GPU (I5);
- buffer e uniform geram programas diferentes. O Pamella usa uma forma só, para não compilar duas
  vezes;
- a transparência continua sendo o ponto difícil (I4).

### 7.5 `canvas(...)`

```cs
Chart = canvas((Graphics g, List<float> values) => { /* ... */ });
```

Criado no `Design`. O `Graphics` vem do contexto, é reconhecido pelo tipo e fica fora da chamada. As
operações gravam instâncias nos shaders embutidos do Pamella, com as mesmas regras de lote.

- **Operações:** `FillRectangle`, `FillEllipse`, `DrawLine`, `DrawString` e `DrawImage`.
- **Por instância:** a lambda pode usar as propriedades da view (`Width`, `Service`).
- **Custo:** rodar o delegate no CPU a cada repintura da view.
- **Sem `DynamicInvoke` por chamada:** o Pamella compila um invocador para a assinatura uma vez.

### 7.6 `view(...)`

```cs
Form = view(ctx => Column(controls: [
    TextBox().Bind(t => t.Text == User),
    Button(text: "Entrar", click: Login),
]));
```

- Criado no `Design`, com o construtor rodando na primeira vez que a instância usa o recurso.
- A raiz vira filha da view dona.
- `Form()` no `Paint` marca onde ela é pintada. Se não for chamada, não aparece nem recebe clique.
- **Roda de novo na reconstrução,** para ligar propriedades e métodos à instância nova.
- `ctx.Owner` dá acesso à view dona (decisão C23).

### 7.7 `Draw(obj)`

**Interpretação minha (decisão C36):** o `Draw(obj)` citado na sua proposta não existe no plano
anterior, e o `Draw()` do esqueleto atual é outra coisa (pinta a view). Proponho que seja só
**associação**:

```cs
void Design() => Logo = Draw(() => Image.Load("logo.png"));
```

- registra o recurso como pertencente à view, e o libera quando ela morre de verdade;
- na reconstrução, devolve o recurso já carregado em vez de carregar de novo;
- **não desenha e não invalida.** Quem desenha é o `Paint`.

### 7.8 Camadas

| camada | o que vai nela |
|---|---|
| `Content` | a árvore da tela |
| `Overlay` | menus, combos abertos, tooltips |
| `Modal` | diálogos e o véu atrás deles |
| `Drag` | o que está sendo arrastado |
| `Debug` | contornos de layout e contadores |

Uma view escolhe a camada com `Layer = Layer.Overlay`, e os filhos herdam. É o que deixa um menu
sair por cima de um container que recorta. A mesma ordem serve ao hit-test: o clique vai para quem
está na frente.

### 7.9 O que o desenho explícito custa

Com os argumentos passados à mão no `Paint`, mudar `Fill` e invalidar roda de novo o `Paint` daquela
view, em vez de escrever um float direto no buffer como no primeiro desenho do plano.

- **O custo é pequeno e local:** uma view, algumas chamadas, alguns slots. **(medir.)**
- **O atalho pode voltar depois** sem mudar a sintaxe, como um argumento vivo
  (`Background(Rect(w, h), () => Fill)`), que o Pamella religaria ao slot da instância.

---

## 8. Invalidação

### 8.1 Regra geral

**Decisão:** desenho e invalidação são explícitos por padrão. Mudar uma propriedade comum não faz
nada na tela. Há dois caminhos:
- `Invalidate()`, chamado à mão;
- `[InvalidateOnChange]`, declarado na propriedade.

`Invalidate()` **só marca**: nunca roda `Paint`. Várias marcas antes do frame viram uma repintura.

### 8.2 `[InvalidateOnChange]`

```cs
public interface MyButton : View
{
    [InvalidateOnChange] string Text { get; set; }
    [InvalidateOnChange] bool Selected { get; set; }
    int InternalCounter { get; set; }
}
```

**O ponto que muda a implementação: o `bind` não passa pelo setter.** **[conferido]**
`DefaultBindBehavior` faz `Box.SetInner(esquerda, direita)`, e a propriedade passa a ler a outra
box. Um `Invalidate()` escrito só no setter gerado **não veria as mudanças vindas de binding**.

Por isso a geração observa a **box**, que já avisa por `INotifyBox`:

```cs
// gerado pelo Pamella, na parte partial da concreta
public partial class MyButtonConcrete
{
    protected override void WireInvalidation()
    {
        Watch<string>(nameof(Text));
        Watch<bool>(nameof(Selected));
    }
}
```

O `Watch` lê a propriedade com `BoxTracker`, assina todas as boxes lidas, guarda o último valor e,
a cada aviso, compara com `EqualityComparer<T>.Default` antes de invalidar.

| regra pedida | como fica | situação |
|---|---|---|
| padrão continua explícito | sem atributo, nada acontece | **[comprovado]** |
| invalida quando o valor muda | setter e box | **[comprovado]** |
| funciona com `bind` | fonte comum **e ternário** | **[comprovado]** |
| valor igual não invalida | comparação no `Watch`, também ao criar um bind | **[comprovado]** |
| respeita o ciclo de vida | antes de montar não marca; ao desmontar desassina | **[comprovado]** |
| consolida antes do frame | marca única por view | [desejado] |
| funciona após reconstrução | `WireInvalidation` roda na instância nova | **[comprovado]** |
| não executa `Paint` | só marca | [desejado] |

**Diagnósticos do gerador:**
- atributo numa propriedade com corpo: erro, porque o Blindness não controla o armazenamento dela;
- atributo fora de uma `View`: erro.

**Recursão:** invalidar a própria view durante o `Paint` vale para o frame seguinte. Em Debug, se
isso se repetir por muitos frames, o Pamella avisa.

**Quem gera (decisão C37):** você pediu o gerador do Blindness. Propus o do Pamella porque ele
alcança o mesmo resultado sem mudar a lib, já que a concreta é `partial` e o armazenamento continua
sendo o do Blindness.

### 8.3 Pai e filhos

Invalidar uma view não invalida as outras. Se a `Area` de um filho muda, o pai só refaz o layout se
for avisado: `Parent.Invalidate()`, ou um container pronto que observe os filhos.

---

## 9. Ciclo de vida

### 9.1 Ordem

```
criar     construtor → Deps
montar    Shaders (uma vez por tipo) → contexto → Design → Behaviour → Load (só contexto novo)
viver     eventos, Tick, Invalidate → Paint
morrer    Unload
```

**Montar** é entrar na árvore. O Blindness chama `Node.Load()` na criação **[conferido]**, mas o
`ViewNode` usa esse momento só para a preparação interna. O `Load` do dev roda na montagem, o que
resolve dois problemas: ele já vê os argumentos da função construtora, e o hot reload consegue
evitá-lo. O `Ready` do plano anterior deixa de existir.

### 9.2 Contratos

| função | quando | quantas vezes | para quê | não fazer |
|---|---|---|---|---|
| `Deps(...)` | criação | por instância | receber serviços | lógica |
| `Shaders()` | abertura ou primeira instância | por tipo | criar shaders | ler dados da instância |
| `Design()` | montagem e reconstrução | por instância construída | `canvas`, `view`, `Draw(obj)` | condicionar a estado que muda |
| `Behaviour()` | montagem e reconstrução | por instância construída | `bind`, eventos, ligações | carregar dados; é síncrono |
| `Load()` | primeira montagem do contexto | por contexto | dados e recursos, `void` ou `Task` | ligar eventos |
| `Paint()` | view invalidada | muitas | desenhar | mudar estado |
| `Tick(float dt)` | a cada passo, se existir | muitas | tempo e animação | assumir que redesenha |
| `Unload()` | morte definitiva | por contexto | encerrar e liberar | tratar reconstrução |

### 9.3 Contexto implícito do `Behaviour`

Como o `Behaviour` é síncrono, o Pamella abre um contexto na thread durante a chamada, como o
`RenderContext` do Radiance. Por isso `bind(...)` e `OnMouseDown += ...` não precisam dizer de quem
são, e tudo é desfeito quando a instância sai. Fora do `Behaviour`, usá-los é erro claro.
**[comprovado]**

### 9.4 Binding em interfaces

**O `Bind` padrão falha com propriedades de interface.** **[comprovado]:**
`z.Bind(() => z.Text == z.Source)` lança `InvalidBindingException`, porque o `[Binding]` está na
concreta e a expressão aponta para a interface.

**Solução sem mudar o Blindness [comprovado]:** o Pamella instala, na abertura, um elo próprio com
`Binding.SetBehaviour`, nos dois lados da expressão. Ele trata propriedades abstratas de interfaces
`INode`, e os elos padrão continuam atrás.

### 9.5 `Load` e `Unload` sem `On`

`INode` já declara os dois. Declarar corpos com esses nomes esconde os originais e gera o aviso
CS0108. O `ViewNode` chama os corpos diretamente, sem despacho virtual. **[comprovado]** O aviso
fica para a decisão C29.

### 9.6 `Tick`

- Só views montadas que declaram `Tick` entram na lista.
- `Tick` não invalida nada por si.
- **Limite do Radiance [conferido]:** em `OnDemand`, o `OnFrame` só roda quando há frame. Enquanto
  houver view com `Tick`, o Pamella precisa acordar a janela, e cada acordada também repete o
  `OnRender`, sem rodar `Paint`. Sem nenhuma view com `Tick`, a janela volta a dormir (decisão C39).

### 9.7 `Deps` e recursos de GPU

- **`Deps`** declara as dependências como parâmetros, e o Blindness preenche as propriedades de mesmo
  nome. **(validar com serviço comum registrado no app.)**
- **Carregamento duplicado não acontece** por construção: `Shaders` é por tipo, `Deps` é por
  instância e `Load` é por contexto.
- **Slots** de uma view são devolvidos quando ela sai da árvore.
- O dev não escreve `Dispose` de recursos de GPU.

---

## 10. Eventos

### 10.1 Registro

Em `Pamella.Utils`, como eventos estáticos contextuais, no mesmo estilo do `Window` do Radiance:

```cs
void Behaviour()
{
    OnMouseDown += button => Pressed = true;
    OnMouseUp += button => Pressed = false;
    OnKeyDown += (key, modifier) => { if (key == Input.Enter) Submit(); };
}
```

As inscrições pertencem à instância e são desfeitas ao desmontar. **[comprovado]**

### 10.2 Contratos

Assinaturas do `Window` **[conferido]**:

| evento | no `Window` | na view | quem recebe |
|---|---|---|---|
| `OnKeyDown`, `OnKeyUp` | `Action<Input, Modifier>` | igual | foco, sobe aos pais |
| `OnTextInput` | `Action<string>` | igual | foco, sobe aos pais |
| `OnMouseMove` | `Action<(float x, float y)>` | igual, em coordenadas locais | view sob o cursor, sobe |
| `OnMouseDown`, `OnMouseUp` | `Action<MouseButton>` | igual | view sob o cursor, sobe |
| `OnMouseWheel` | `Action<float>` | igual | view sob o cursor, sobe |
| `OnMouseEnter`, `OnMouseLeave` | `Action` (janela) | `Action` (região da view) | cada view que ganhou ou perdeu o cursor, sem subir |
| `OnResize` | `Action<int, int>` (janela) | `Action<float, float>` (`Area` mudou) **(proposta)** | a própria view |
| `OnFocusChange` | `Action<bool>` (janela) | `Action<bool>` (foco da view) | a view |
| `OnFileDrop` | `Action<string[]>` | igual | view sob o cursor, sobe |

O `OnMouseWhell` do Radiance é um nome antigo e não entra.

### 10.3 Regras

- **Detecção:** percorre as listas de pintura de frente para trás, respeitando scissor e `Offset`, e
  testa a `Area`. Sem `Area` declarada, usa o retângulo gravado. Quem não foi pintado não recebe.
- **Propagação:** do alvo para os pais. `StopPropagation()` interrompe, com o mesmo contexto
  implícito durante o handler.
- **Captura:** entre `OnMouseDown` e `OnMouseUp`, a view que recebeu o clique continua recebendo
  movimento e soltura.
- **Foco:** clique numa view focável ou Tab. Perder o foco da janela gera `false` na view focada.
- **Sem desenho automático:** evento não invalida nada. **Limite [conferido]:** em `OnDemand`, a
  entrada acorda a janela e gera um frame, que repete as listas sem rodar `Paint`.

---

## 11. Hot reload

### 11.1 Duas árvores

| árvore | o que contém | no reload |
|---|---|---|
| **ViewTree** | as instâncias de `View`, de tipos gerados | é reconstruída |
| **ContextTree** | um nó por componente lógico, com dados, estado do `Load`, recursos de `Draw`, foco e rolagem | é preservada |

**Identidade de um nó:** contexto do pai, posição (nome do recurso `view` ou índice entre os filhos),
nome completo do tipo e `key:` opcional. Mesma identidade, mesmo contexto.

### 11.2 Sequência

1. O reload do Blindness produz o assembly novo.
2. `Shaders` roda de novo para os tipos novos.
3. A árvore é refeita a partir da raiz, com `Node.New`.
4. Cada instância nova procura o seu contexto.
5. **Contexto achado:** recebe os dados, roda `Design` e `Behaviour`, e **não** roda `Load`.
6. **Contexto não achado:** é componente novo e segue a montagem normal, com `Load`.
7. Instâncias antigas são desligadas (bindings, eventos, `Watch`), **sem `Unload`**.
8. **Contextos que ninguém reclamou** morreram: roda o `Unload` da instância antiga.
9. Tudo é invalidado uma vez.

**[comprovado]** no modelo simulado: instância nova sem `Load` e sem `Unload`, `Behaviour` executado
de novo, e `Unload` só na morte real.

### 11.3 Três formas de sair de cena

| situação | `Behaviour` e `Design` da nova | `Load` | `Unload` |
|---|---|---|---|
| **reconstrução** (hot reload) | rodam | não | não |
| **substituição** (mesma posição, outro tipo ou `key`) | rodam | sim, na nova | sim, na antiga |
| **descarte definitivo** | — | — | sim |

### 11.4 Dados

| tipo do valor | como passa |
|---|---|
| mesmo `Type` (`string`, `int`, tipos de libs) | a instância nova **compartilha a `ValueBox`** da antiga **[comprovado]** |
| tipo recompilado (`Product`, `List<Product>` do app) | o `Type` é outro: **decisão C35** |
| propriedade com binding | não passa: o `Behaviour` refaz |
| propriedade nova | valor padrão, porque o `Load` não roda |
| `dynamic` de recurso | não passa: `Shaders` e `Design` preenchem |

**Atenção:** o `Node.CopyNonNodeValues` do Blindness ignora em silêncio propriedades cujo tipo
mudou **[conferido]**. Como o assembly inteiro é recompilado, `List<Product>` cairia nesse caso. Com
o `Load` sem rodar, a lista **sumiria**. Por isso o Pamella não depende só dessa cópia.

### 11.5 `Load` em andamento

- **Continua rodando.** O `await` mantém a instância antiga, mas ela compartilha a box com a nova, e
  o resultado aparece na tela. **[comprovado]**
- **Tipo recompilado:** o valor é migrado uma vez, quando a `Task` termina, pela mesma regra da C35.
- **Morte real:** o contexto cancela um `CancellationToken`, disponível como `LoadToken`. Escritas
  depois disso não têm efeito.
- **Exceção:** é registrada no contexto e reportada, sem derrubar a reconstrução.

### 11.6 Recursos

| recurso | no reload do Blindness | em edição de corpo pelo .NET |
|---|---|---|
| shaders | recriados, porque os tipos são novos; o GLSL recompila no primeiro desenho | só os tipos alterados |
| `canvas`, `view` | recriados pelo `Design` | recriados |
| `Draw(obj)` | reaproveitados do contexto | reaproveitados |

**Reaproveitar programas compilados** entre assemblies exige cache por texto de GLSL, que o Radiance
não oferece. Sem isso, o custo é de compilação em desenvolvimento. **(medir.)**

### 11.7 Os dois mecanismos reais

**Blindness (principal) [conferido]:** recompila todos os `.cs` da pasta com Roslyn e carrega um
assembly novo. Aceita qualquer edição. Limites:
- roda só o gerador do Blindness, então o Pamella precisa de um `AssemblyCompiler` próprio
  (`GetNewAssembly` é `virtual`) que inclua o gerador dele, com o código do gerador também compilado
  na lib, como o Blindness faz;
- não recompila projetos referenciados;
- usa opções fixas de compilação, não as do `.csproj`;
- estado em campos estáticos se perde.

**.NET (`dotnet watch`, Visual Studio):** a documentação da Microsoft lista "modificar interface",
mudar assinaturas, adicionar membros abstratos ou virtuais e mudar atributos como **não suportados**.
Corpos de método e de lambda são suportados.

| edição | Blindness | .NET |
|---|---|---|
| corpo de `Paint`, `Behaviour`, `Design`, lambdas | sim | sim **(validar em membro padrão de interface)** |
| corpo de `Shaders` | sim | sim, com `MetadataUpdateHandler` refazendo o tipo |
| nova propriedade ou recurso | sim | **não, reinício** |
| novo componente | sim | sim na interface nova; a função gerada **(validar)** |
| `[InvalidateOnChange]`, `[Default]` | sim | **não, reinício** |
| mudar corpo de `Load` | aplica, mas só roda em contexto novo | igual |

Para testar um `Load` editado, `ReloadData()` roda o `Load` do contexto de novo.

---

## 12. Janela e superfície

**Recomendação (C26): uma superfície por janela, e fullscreen como opção.**

- **O que você quer vem da superfície única, não do fullscreen:** coordenadas, profundidade, recorte
  e composição unificados valem igual numa janela comum.
- **Fullscreen obrigatório custaria:** barra de tarefas e alt-tab estranhos, nenhum app pequeno (um
  diálogo, uma ferramenta), nada lado a lado com outro programa, e desenvolvimento mais chato, com o
  editor atrás do app.
- **O Radiance já tem os quatro modos** (`WindowMode`), então é configuração:
  `App.Run<Tela>(app => app.Mode = WindowMode.Fullscreen)`. O padrão é `Windowed`.
- **Limite que fica:** menus, tooltips e diálogos são camadas da mesma superfície e não saem da
  janela. Várias janelas do sistema ficam para depois.

---

## 13. Exemplos de app

Todos **[desejado]**.

### 13.0 O projeto

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Pamella" Version="3.0.0-preview1" />
  </ItemGroup>
</Project>
```

### 13.1 Contador

```cs
App.Run<Counter>();

public interface Counter : View
{
    int Clicks { get; set; }

    dynamic Content { get; set; }

    void Design()
    {
        Content = view(ctx => Column(
            spacing: 8,
            controls: [
                Label().Bind(l => l.Text == $"{Clicks} cliques"),
                Button(text: "+1", click: () => Clicks++),
            ]
        ));
    }

    void Paint() => Content();
}
```

`Clicks` não precisa do atributo: `Counter` não desenha esse valor. O `Text` do `Label`, sim, é
`[InvalidateOnChange]`, então só o `Label` é repintado. **(validar `Bind` com string interpolada.)**

### 13.2 Login: serviços, `await`, navegação

```cs
App.Run<LoginScreen>(app =>
{
    app.Title = "Acme";
    app.Size = (420, 360);
    app.Services.Add<AuthService>();
});

public interface LoginScreen : View
{
    AuthService Auth { get; set; }
    string User { get; set; }
    string Password { get; set; }
    string Error { get; set; }
    bool Busy { get; set; }

    void Deps(AuthService Auth);

    dynamic Form { get; set; }

    void Design()
    {
        Form = view(ctx => Column(padding: 24, spacing: 12, controls: [
            Label(text: "Entrar", style: Theme.Title),
            TextBox(placeholder: "Usuário").Bind(t => t.Text == User),
            TextBox(placeholder: "Senha", password: true).Bind(t => t.Text == Password),
            Label(style: Theme.Error).Bind(l => l.Text == Error),
            Button(text: "Entrar", click: Login).Bind(b => b.Enabled == !Busy),
        ]));
    }

    void Paint() => Form();

    async void Login()
    {
        Busy = true;
        Error = null;
        try
        {
            var session = await Auth.Login(User, Password);
            App.Push<HomeScreen>(home => home.Session = session);
        }
        catch (AuthException ex)
        {
            Error = ex.Message;
        }
        finally
        {
            Busy = false;
        }
    }
}
```

### 13.3 Lista carregada

```cs
public interface Products : View
{
    Catalog Catalog { get; set; }
    string Filter { get; set; }
    List<Product> Items { get; set; }

    void Deps(Catalog Catalog);

    dynamic Content { get; set; }

    void Design()
    {
        Content = view(ctx => Column(controls: [
            TextBox(placeholder: "Filtrar").Bind(t => t.Text == Filter),
            ListView(item: (Product p) => Row(controls: [
                Label(text: p.Name),
                Label(text: $"{p.Price:C}"),
            ])).Bind(l => l.Items == Items),
        ]));
    }

    void Behaviour()
        => bind(() => Items == Catalog.Search(Filter));

    async Task Load() => await Catalog.Fetch(LoadToken);

    void Paint() => Content();
}
```

Um `ListView` recicla linhas e fica responsável por reagir a `Items`.

### 13.4 Um componente do zero

```cs
public interface Pill : View
{
    [InvalidateOnChange] Vec4 Fill { get; set; }
    [InvalidateOnChange] string Text { get; set; }
    Action OnClick { get; set; }

    dynamic Body { get; set; }
    dynamic Caption { get; set; }

    void Shaders()
    {
        Body = shader((vec4 fillColor) =>
        {
            color = fillColor;
            fill();
        });
    }

    void Design()
        => Caption = view(ctx => Label(x: 12, y: 6).Bind(l => l.Text == Text));

    void Behaviour()
        => OnMouseDown += button => OnClick?.Invoke();

    void Paint()
    {
        var area = Area;
        Body(Rect(area.Width, area.Height), Fill);
        Caption();
    }

    Area Area
    {
        get
        {
            var text = Measure(Caption);
            return new(0, 0, text.Width + 24, text.Height + 12);
        }
    }
}
```

### 13.5 Testando uma tela

```cs
var ui = TestHost.Open<LoginScreen>();
ui.Find<TextBox>("Usuário").Type("ana");
ui.Find<Button>("Entrar").Click();
Assert.Equal("ana", ui.Screen.User);
```

O host de testes roda sem GPU, sobre o backend falso do Radiance, e deixa conferir a lista de pintura
de cada view.

### 13.6 O que o dev não escreve

- loop de janela;
- código para juntar desenhos em lote ou ordenar profundidade;
- passagem de `(x, y, z)` de pai para filho;
- marshaling de thread;
- `Dispose` de recursos de GPU;
- código para religar eventos depois do hot reload.

**(validar)** antes do M1: `Bind` com string interpolada, `Deps` com serviço comum e o custo do
`Watch` por propriedade.

---

## 14. Decisões

### 14.1 Tomadas

| | decisão | situação |
|---|---|---|
| D1 | níveis misturáveis: shader, `Graphics`, prontos | vale |
| D2 | um shader, várias instâncias, profundidade para a ordem | vale, com os limites da seção 7.4 |
| D3 | camadas no Pamella | vale |
| D4 | propriedades alimentando buffers diretamente | **substituída:** chegam pelo `Paint` |
| D5 | compilação de GLSL no primeiro uso, por ora | vale; `Shaders` já antecipa a criação |
| D6 | nenhuma mudança nas libs por agora | vale: tudo tem contorno testado ou proposto |
| D7 | composição decidida por componente | vale |
| D8 | componente de texto central | vale |
| D9 | `Graphics` e shaders no desenho | vale, por `canvas` e `shader` |
| D10 | invalidação automática por padrão | **substituída por D26** |
| D11 | componentes são interfaces Blindness | vale |
| D12 | declaração numa função, uma vez por tipo | vale em `Shaders`, com `shader(...)` |
| D13 | árvore no estilo do Godot, `View` mínima | vale |
| D14 | recursos declarados e chamados como função | vale |
| D15 | desenho em `Paint()` sem parâmetros | vale |
| D16 | funções com argumentos nomeados | vale, agora geradas (D22) |
| D17 | `Deps` e carregamento preservados | vale |
| D18 | `LoadUI` | **substituída** por `Shaders` e `Design` |
| D19 | funções sem `On` | vale; eventos passam ao `Behaviour` (D27) |
| D20 | separação de responsabilidades | vale, ampliada na D25 |
| D21 | `View` sem tamanho, `Measure` e `Area`, `Clip` explícito | vale |
| **D22** | funções construtoras geradas, com `global using static` | nova |
| **D23** | `shader(...)` criado em `Shaders()`, sobre os mecanismos do Radiance | nova |
| **D24** | polígono público e explícito; `(x, y, z)` contextuais; currying e `skip` preservados | nova |
| **D25** | ciclo `Behaviour`, `Load`, `Unload`, `Shaders`, `Design`, `Paint`, `Tick` | nova |
| **D26** | invalidação explícita, com `Invalidate()` e `[InvalidateOnChange]` | nova |
| **D27** | eventos do `Window` adaptados às views, registrados no `Behaviour` | nova |
| **D28** | hot reload com ViewTree e ContextTree, sem repetir `Load` nem chamar `Unload` | nova |

### 14.2 Abertas

**C31. Geração do wrapper: sem escolha, por decisão sua.** Seção 6.6.

**C32. Onde aplicar o deslocamento**
- **rec:** gerador de código decorado.
- **alt:** `fill` próprio do Pamella.

**C33. Tamanho e lote**
- **rec:** `Rect(w, h)` com cache por tamanho. Simples, e cada tamanho é um draw.
- **alt:** polígono unitário com escala contextual `(sx, sy)` por instância. Junta tamanhos
  diferentes e acrescenta dois valores ao wrapper.

**C34. Eixo `y`**
- **rec:** o do Radiance (origem embaixo, `y` para cima), sem conversão.
- **alt:** origem em cima, com o Pamella invertendo no gerador. Mais natural para UI, e o polígono
  do dev passa a aparecer espelhado em relação ao Radiance.

**C35. Dados de tipos recompilados**
- **rec:** cópia estrutural por nome (propriedades, listas, dicionários), com aviso no que não couber.
- **alt A:** recomendar que os modelos fiquem num projeto referenciado, que não é recompilado.
- **alt B:** perder o valor e permitir `ReloadData()`.

**C36. `Draw(obj)`:** associação, como na seção 7.7, ou outro significado que você tenha em mente.

**C37. Onde gerar `[InvalidateOnChange]`**
- **rec:** gerador do Pamella, na parte `partial`.
- **alt:** estender o gerador do Blindness com um gancho genérico de propriedade, por proposta.

**C38. Valores padrão:** **rec** `[Default(constante)]`. **alt:** ler um inicializador declarado num
membro estático da interface, que aceitaria valores não constantes.

**C39. Acordar para o `Tick`**
- **rec:** temporizador do Pamella chamando `Window.Invalidate()`, com taxa configurável.
- **alt:** `RenderMode.Continuous` enquanto houver `Tick`.

**C4. O que o shader recebe além dos parâmetros.** `pos` já é local à view (seção 6.5).
- **rec:** o Pamella injeta tamanho, posição local no fragmento, os estados (`hovered`, `pressed`,
  `focused`, `disabled`) como `val` 0/1 e o tempo `t`. Com o estado na GPU, um hover custa um float.
- **alt:** só tamanho e posição local, com os estados passados como argumento no `Paint`. Mais
  explícito, e um hover passa a exigir invalidação.

**C5. A DSL de shader dentro do componente**
- **rec:** `using static Radiance.Utils;`, como no Radiance hoje, possivelmente injetado pelo pacote.
- **alt:** a `View` reexpõe a DSL como membros estáticos. Funciona, mas duplica centenas de nomes.

**C6. Nomes do `Graphics`**
- **rec:** os do WinForms (`FillRectangle`, `DrawString`, `DrawImage`), para quem migra.
- **alt:** nomes curtos (`FillRect`, `Text`, `Image`).

**C7. Camadas**
- **rec:** as camadas fixas da seção 7.8.
- **alt:** um inteiro livre (`ZIndex`), mais flexível e mais fácil de virar bagunça.

**C10. Direção do binding**
- **rec:** um único `Bind`, que liga os dois lados quando a expressão é reversível.
- **alt:** separar `.Bind` (ida e volta) de `.Show` (só ida).

**C11. Eventos vindos de fora do componente**
- **rec:** propriedades `Action` com `On` (`OnClick`), que viram o argumento `click:`.
- **alt:** `event` do C#, como no WinForms.

**C12. Nomes de layout**
- **rec:** `Column`, `Row`, `Grid`, `Dock` e `Stack`.
- **alt:** `StackPanel` e `DockPanel`, do WPF.

**C13. Navegação, tema e `using`**
- **navegação (rec):** pilha com `App.Push<T>(init)` e `App.Pop()`, e diálogos com
  `await App.Dialog<T, R>()`;
- **tema (rec):** tokens e estilos como objetos C#, seguindo o tema do sistema;
- **`using` (rec):** o pacote injeta `global using Pamella;`, com opt-out.

**C14. `await` na UI**
- **rec:** `SynchronizationContext` no thread da janela.
- **alt:** só `Dispatcher.Post`.

**C15. Plataforma**
- **rec:** Windows primeiro, sem fechar portas para Linux e macOS.

**C17. Componentes base compartilhados**
- **rec:** texto, painel, imagem e ícone. Os prontos os usam sempre que der, e cada exceção é
  justificada na proposta do componente.
- **alt:** só o texto como base, com cada pronto tendo shader próprio.

**C21. Coordenadas dentro do `Paint`**
- **rec:** locais, com o contexto somando a posição. Mover ou rolar não repinta.
- **alt:** globais (`Clip(X, Y, Width, Height)`). Mais literal, e mover um ancestral repinta todos os
  descendentes.

**C22. O que `X` e `Y` significam**
- **rec:** posição efetiva, com o deslocamento dos ancestrais (seção 4.2).
- **alt:** posição lógica (`Parent.X + LocalX`), com um segundo par para a efetiva.

**C23. O `ctx` do `view(...)`**
- **rec:** só `Owner`. O que muda fica em componentes como `ListView` e em condicionais no `Paint`.
- **alt:** `ctx.Watch(() => Items)`, que roda o construtor de novo quando o valor muda.

**C24. View sem `Paint`**
- **rec:** não desenha nada.
- **alt:** pinta os `view(...)` declarados. Poupa uma linha e esconde a ordem de desenho.

**C26. Janela**
- **rec:** superfície única por janela, modo configurável, padrão `Windowed` (seção 12).
- **alt:** fullscreen obrigatório.

**C29. Aviso CS0108 em `Load` e `Unload`**
- **rec:** o pacote cala o aviso só para views, com um supressor de diagnóstico. **(validar.)**
- **alt A:** o dev escreve `new`.
- **alt B:** outros nomes, que não batam com o `INode`.

**C30. `Area` automática**
- **rec:** o retângulo que envolve o que o `Paint` gravou, filhos incluídos.
- **alt:** só declarada. Mais previsível, e todo componente em container precisa declarar.

**Encerradas:**
- **C2** (D22), **C16** (D26), **C20** (D21) e **C27** (`dynamic`, com tipos opcionais, seção 5.6);
- **C25:** o `Load` passou para a montagem;
- **C28:** o `Design` é por instância, então não há religação de delegates;
- **C3, C18, C19:** os argumentos são passados no `Paint`, e não há shader automático;
- **C8, C9:** a árvore vem de `view(...)`, e o conteúdo condicional fica no `Paint`.

---

## 15. Decisões de implementação

**I1. Host.** Segue a Pamella-003: `AppBehaviour` próprio, tudo no thread da janela, `OnDemand`,
`Dispatcher` com `SynchronizationContext` e exceção de handler sem derrubar o app em Debug. Na
abertura, instala o decorador do gerador de código e o `Binding.SetBehaviour`.

**I2. Estoque de instâncias**
- um estoque por (definição, polígono), com um buffer por componente público mais `x`, `y`, `z`;
- slots com lista de livres, e os livres ficam com tamanho zero;
- capacidade em potências de 2, para a contagem vista pelo Radiance mudar raramente (Radiance-006);
- compactação quando a ocupação cai abaixo de 25%. Compactar move slots, e as listas passam a apontar
  para os novos.

**I3. Observação de propriedades**
- **rec:** `Watch<T>` sobre `INotifyBox` e `BoxTracker`, testado com `bind`. **(medir o custo por
  instância.)**
- **alt:** um tipo de box do Pamella, sem evento, para o caso sem binding, se a medição pedir.

**I4. Profundidade e transparência**
- **rec:** duas passadas. A opaca usa depth test e sai em lotes, descartando os pixels com alfa abaixo
  de 1. A translúcida vai de trás para frente, só para o que tem alfa (bordas suavizadas, sombras,
  cores translúcidas), juntando instâncias vizinhas do mesmo shader.
- **custo:** a borda suavizada sai na segunda passada, e telas típicas têm pouca coisa translúcida
  empilhada.
- **alt A:** uma passada em ordem de pintura. Corrige a transparência e quebra os lotes.
- **alt B:** bordas sem suavização, só com `discard`. Uma passada, com serrilhado.

**I5. Recorte.** **rec:** scissor, como você definiu, com um lote por região. **alt, como otimização
futura:** recorte por instância no shader, se a medição mostrar muitos lotes.

**I6. Texto**
- **rec:** um quadrado por glifo, com atlas numa textura (de preferência SDF, que escala sem perder
  nitidez). Depende de render para textura (Radiance-014) ou de rasterizar no CPU.
- **até lá:** um polígono por string, com o cache do Radiance.
- **alt:** triangulação por glifo (Radiance-004), sem textura e com mais vértices.

**I7. Lista de pintura**
- por view: instâncias, scissor, deslocamentos e marcas de filhos;
- regravar libera os slots antigos da view e ocupa novos;
- views com `Paint` grande ganham um trecho contíguo, para regravar de uma vez;
- um shader que não pode entrar em lote vira um ponto de corte: os lotes anteriores saem antes.

**I8. Membros e recursos**
- membros por convenção são procurados uma vez por tipo, e `Load` e `Unload` são chamados sem
  despacho virtual;
- no carregamento do tipo, viram erro claro: shader que captura a instância e uso de `OnLoad` ou
  `OnUnload` numa view;
- cada `canvas` tem um invocador compilado por assinatura.

**I9. Layout e entrada.** Seguem a Pamella-005 (`Measure` e `Arrange`) e a Pamella-007, adaptadas às
seções 4 e 10.

**I10. Hot reload.** Seção 11: compilador próprio, ContextTree, foco e rolagem restaurados pela
identidade do nó, com `key:` opcional.

**I11. Testes, pacote e versões**
- **testes:** host sem GPU sobre o backend falso do Radiance, com checagem de layout, eventos e listas
  de pintura, e testes de pixel no estilo do `window-test`;
- **pacote:** um assembly, namespace `Pamella`, `net11.0`, Radiance e Blindness pelo NuGet;
- **versões:** `3.0.0-previewN`.

---

## 16. O que as libs podem oferecer

**Nada bloqueia (D6).** Cada item tem contorno no Pamella, e melhoraria o resultado:

| lib | o quê | contorno |
|---|---|---|
| Radiance | curry compartilhando programas compilados | aplicação própria do Pamella |
| Radiance | aceitar delegates de `Expression.Compile()` | só se a C31 for por esse caminho |
| Radiance | `PreLoad` (Radiance-017) | draw com scissor vazio |
| Radiance | cache de programa por texto de GLSL | recompilar no reload |
| Radiance | cache por instâncias (Radiance-006) | potências de 2 |
| Radiance | render para textura (Radiance-014) | polígono por string |
| Radiance | escala do monitor (Radiance-021) | 100% até lá |
| Blindness | bind de propriedade de interface | elo próprio por `SetBehaviour` |
| Blindness | geradores extras no reload | `AssemblyCompiler` próprio |
| Blindness | gancho para atributos de propriedade | parte `partial` gerada |
| Blindness | tipo compatível após reload (Blindness-014) | comparar pelo nome |

**Para depois:** compilar um render sem desenhar e cache de programa em disco (seção 6.7).

---

## 17. Roteiro

| marco | entrega | versão |
|---|---|---|
| M0 Fundação | decisões, `ARCHITECTURE.md`, projeto compilando, janela vazia | preview1 |
| M1 Componente | `View`, `ViewNode`, ciclo de vida, elo de binding, `Invalidate`, `[InvalidateOnChange]`, funções geradas | preview2 |
| M2 Shaders | experimentos da C31, definição e aplicação, wrapper, gerador decorado, `Paint` gravado, lotes e profundidade | preview3 |
| M3 Árvore | `view`, `Paint(view)`, `Clip`, `Offset`, `Area`, `Measure`, camadas | preview4 |
| M4 Entrada | eventos, hit-test, foco, `Tick`, layouts | preview5 |
| M5 Hot reload | ContextTree, compilador próprio, migração de dados | preview6 |
| M6 Prontos | texto, `canvas`, `Graphics`, `Label`, `Button`, `TextBox`, tema | preview7 |
| M7 App real | `ListView`, diálogos, DPI, acessibilidade, host de testes | preview8 |

Os shaders vêm antes da árvore de propósito: o M2 mede o desenho mais arriscado (`Paint` gravado →
slots → uma chamada) antes de construir o resto em cima.

O hot reload vem antes dos prontos porque a ContextTree define a identidade dos componentes, e é
mais barato validá-la com poucos deles.

---

## 18. Próximos passos

1. **Você revisa** as decisões da seção 14.2. As que mais mudam o resto são C33, C34, C35, C36 e C37.
2. **Probe do gerador:** um `IIncrementalGenerator` real, conferindo IntelliSense sem compilar e
   referências entre projetos.
3. **Probe com GPU:**
   - os experimentos da C31;
   - o custo de compilar um shader, de regravar uma view e de uma chamada `dynamic`;
   - quantas instâncias de um SDF uma chamada aguenta a 60 fps;
   - lotes por scissor, transparência e o `Measure` automático com seus ciclos.
4. **Probe de hot reload** com recompilação real: identidade dos contextos e migração de dados.
5. **Reescrevo as propostas do Pamella:** a Pamella-004 (árvore) e a Pamella-006 (pintura) mudam de
   centro, e entram propostas para shaders, funções construtoras, invalidação, camadas e ContextTree.
6. **Implemento o M0.**
