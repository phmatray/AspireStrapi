// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import mermaid from 'astro-mermaid';

// https://astro.build/config
export default defineConfig({
	// GitHub Pages project-pages hosting: https://phmatray.github.io/AspireStrapi/
	site: 'https://phmatray.github.io',
	base: '/AspireStrapi',
	integrations: [
		mermaid({
			theme: 'forest',
			autoTheme: true,
		}),
		starlight({
			title: 'AspireStrapi',
			description:
				'A .NET 10 Aspire app with a Strapi 5 headless CMS, GraphQL via StrawberryShake, and a Blazor frontend.',
			social: [
				{
					icon: 'github',
					label: 'GitHub',
					href: 'https://github.com/phmatray/AspireStrapi',
				},
			],
			sidebar: [
				{ label: 'Overview', slug: 'overview' },
				{ label: 'Architecture', slug: 'architecture' },
				{
					label: 'Backend',
					items: [
						{ label: 'Strapi GraphQL setup', slug: 'strapi-graphql' },
						{ label: 'StrawberryShake client', slug: 'strawberryshake' },
					],
				},
				{
					label: 'Operations',
					items: [
						{ label: 'Running locally with Aspire', slug: 'running-locally' },
						{ label: 'Deploying to OrbStack', slug: 'deploying' },
					],
				},
			],
		}),
	],
});
