const config = {
    branches: ['main', 'feature/68-automatic-release-workflow'],
    plugins: [
      '@semantic-release/commit-analyzer',
      '@semantic-release/release-notes-generator',
      '@semantic-release/npm',
      ['@semantic-release/git', {
        'assets': ['package.json'],
        'message': 'chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}'
      }],
      '@semantic-release/github'
    ]
  };
  
  module.exports = config;