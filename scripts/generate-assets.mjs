#!/usr/bin/env node
/**
 * MedControl — Asset Generator
 *
 * Gera todos os ícones e assets de marca a partir do SVG master.
 * Uso: node scripts/generate-assets.mjs
 */

import sharp from 'sharp';
import { readFileSync, mkdirSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, '..');

// ── SVGs ────────────────────────────────────────────────────────────────────

/** Badge completo: fundo laranja + "M" branco */
const badgeSvg = (size) => Buffer.from(`
<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 100 100">
  <rect width="100" height="100" rx="22" fill="#F97316"/>
  <text x="50" y="70" text-anchor="middle" dominant-baseline="auto"
        font-family="Arial, Helvetica, sans-serif" font-weight="800"
        font-size="56" fill="#FFFFFF">M</text>
</svg>`);

/** Foreground: "M" branco em fundo transparente, com 20% de padding */
const foregroundSvg = (size) => {
  const pad = size * 0.20;
  const inner = size - pad * 2;
  return Buffer.from(`
<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
  <text x="${size / 2}" y="${size * 0.72}" text-anchor="middle" dominant-baseline="auto"
        font-family="Arial, Helvetica, sans-serif" font-weight="800"
        font-size="${inner * 0.75}" fill="#FFFFFF">M</text>
</svg>`);
};

/** Splash: "M" branco em fundo transparente, centralizado */
const splashSvg = (size) => Buffer.from(`
<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
  <text x="${size / 2}" y="${size * 0.65}" text-anchor="middle" dominant-baseline="auto"
        font-family="Arial, Helvetica, sans-serif" font-weight="800"
        font-size="${size * 0.45}" fill="#FFFFFF">M</text>
</svg>`);

/** Monochrome: "M" branco em fundo preto */
const monochromeSvg = (size) => Buffer.from(`
<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 100 100">
  <rect width="100" height="100" fill="#000000"/>
  <text x="50" y="70" text-anchor="middle" dominant-baseline="auto"
        font-family="Arial, Helvetica, sans-serif" font-weight="800"
        font-size="56" fill="#FFFFFF">M</text>
</svg>`);

// ── Helpers ──────────────────────────────────────────────────────────────────

async function savePng(svgBuffer, outputPath, width, height) {
  mkdirSync(dirname(outputPath), { recursive: true });
  await sharp(svgBuffer).resize(width, height).png().toFile(outputPath);
  console.log(`  ✓  ${outputPath.replace(root + '/', '')}  (${width}×${height})`);
}

async function solidColor(color, outputPath, width, height) {
  const [r, g, b] = color.match(/\w\w/g).map((h) => parseInt(h, 16));
  mkdirSync(dirname(outputPath), { recursive: true });
  await sharp({
    create: { width, height, channels: 3, background: { r, g, b } },
  })
    .png()
    .toFile(outputPath);
  console.log(`  ✓  ${outputPath.replace(root + '/', '')}  (${width}×${height}) solid`);
}

// ── Main ─────────────────────────────────────────────────────────────────────

(async () => {
  console.log('\n🎨  MedControl — gerando assets de marca\n');

  const web = resolve(root, 'apps/web/public');
  const mobile = resolve(root, 'apps/mobile/assets');

  // ── Web ──────────────────────────────────────────────────────────────────
  console.log('Web:');
  await savePng(badgeSvg(512), `${web}/favicon-16.png`, 16, 16);
  await savePng(badgeSvg(512), `${web}/favicon-32.png`, 32, 32);
  await savePng(badgeSvg(512), `${web}/apple-touch-icon.png`, 180, 180);
  await savePng(badgeSvg(512), `${web}/icon-192.png`, 192, 192);
  await savePng(badgeSvg(512), `${web}/icon-512.png`, 512, 512);

  // ── Mobile ───────────────────────────────────────────────────────────────
  console.log('\nMobile:');
  await savePng(badgeSvg(1024), `${mobile}/icon.png`, 1024, 1024);
  await savePng(badgeSvg(48), `${mobile}/favicon.png`, 48, 48);
  await savePng(splashSvg(1024), `${mobile}/splash-icon.png`, 1024, 1024);
  await savePng(foregroundSvg(512), `${mobile}/android-icon-foreground.png`, 512, 512);
  await solidColor('F97316', `${mobile}/android-icon-background.png`, 512, 512);
  await savePng(monochromeSvg(432), `${mobile}/android-icon-monochrome.png`, 432, 432);

  console.log('\n✅  Todos os assets gerados com sucesso!\n');
})();
