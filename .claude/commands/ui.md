# /ui — Novo Componente Angular (Design System)

Use este comando antes de criar ou refatorar **qualquer componente visual** no projeto web.

## Passo 1 — Ler o design system

Antes de escrever qualquer linha de CSS ou template, leia os seguintes arquivos:

- `packages/design-system/src/web/_tokens.scss` — todos os tokens `--mmc-*` disponíveis
- `packages/design-system/src/web/_components.scss` — mixins e classes utilitárias
- `apps/web/src/index.html` — fontes carregadas globalmente
- `apps/web/src/styles.scss` — imports globais e overrides

## Passo 2 — Entender os padrões existentes

Leia pelo menos um componente similar já implementado para entender os padrões visuais e de código em uso:

- Autenticação/onboarding: `apps/web/src/app/auth/login/`
- Seleção/listagem: `apps/web/src/app/tenants/tenant-select/`

## Passo 3 — Implementar

Com o design system mapeado, implemente o componente seguindo as convenções do projeto:

- Standalone + `OnPush` — sem exceções
- Estilos no `styles: []` com SCSS (nesting permitido — `inlineStyleLanguage: "scss"`)
- Ou `styleUrl` externo `.scss` para componentes com template separado
- Spinner de loading: `<mat-spinner>` do Angular Material (já configurado)
- **Nunca** importe fontes externas — as fontes já estão no `index.html`
- **Nunca** use cores hardcoded — sempre `var(--mmc-*)`

## Checklist final

- [ ] Todos os valores de cor, espaçamento, tipografia e sombra usam `var(--mmc-*)`
- [ ] Nenhum `@import` de fonte
- [ ] Estados mapeados: default, hover, focus, error, disabled, loading
- [ ] Spinner usa `<mat-spinner>` (não CSS customizado)
- [ ] Responsivo verificado
- [ ] Pronto para `/tdd`
