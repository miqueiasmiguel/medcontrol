/** @type {import('@commitlint/types').UserConfig} */
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      ['feat', 'fix', 'docs', 'style', 'refactor', 'test', 'chore', 'perf', 'ci', 'revert'],
    ],
    'scope-enum': [
      1,
      'always',
      ['domain', 'app', 'infra', 'api', 'web', 'mobile', 'contracts', 'ci', 'deps', 'auth', 'tenants', 'users', 'payments', 'doctors', 'health-plans', 'procedures', 'members', 'design-system'],
    ],
    'subject-case': [2, 'always', 'lower-case'],
    'subject-max-length': [2, 'always', 100],
    'body-max-line-length': [2, 'always', 200],
  },
};
