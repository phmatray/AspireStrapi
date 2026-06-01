# Creating multi-stage build for production (npm-based, Strapi 5)
FROM node:24-alpine AS build
# Installing libvips-dev for sharp compatibility and build toolchain for
# native modules (better-sqlite3, pg).
RUN apk update && apk add --no-cache build-base gcc autoconf automake zlib-dev libpng-dev nasm bash vips-dev git
ENV NODE_ENV=production

WORKDIR /opt/app
COPY package.json package-lock.json ./
RUN npm config set fetch-timeout 600000 -g && npm ci
COPY . .
RUN npm run build

# Creating final production image
FROM node:24-alpine
RUN apk add --no-cache vips-dev
ENV NODE_ENV=production
WORKDIR /opt/app
COPY --from=build /opt/app ./
ENV PATH=/opt/app/node_modules/.bin:$PATH
RUN chown -R node:node /opt/app
USER node
EXPOSE 1337
CMD ["npm", "run", "start"]
