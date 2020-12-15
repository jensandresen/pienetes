export default class SecretRepository {
  constructor(options) {
    const { db, logger } = options;

    if (!db) {
      throw Error(`Error! Missing required argument "db".`);
    }

    this.db = db;
    this.logger = logger || (() => {});

    this.readSecret = this.readSecret.bind(this);
    this.writeSecret = this.writeSecret.bind(this);
    this.exists = this.exists.bind(this);
  }

  async exists(name) {
    const existingSecret = await this.readSecret(name);
    return !!existingSecret;
  }

  async readSecret(name) {
    const result = await this.db.query(
      `SELECT * FROM secret WHERE name = '${name}'`
    );

    const rows = result.rows || [];

    if (rows.length === 1) {
      return rows[0]["value"];
    }

    return null;
  }

  async writeSecret(name, value) {
    const alreadyExists = await this.exists(name);

    const query = alreadyExists
      ? {
          text: `UPDATE secret SET value = $2 WHERE name = $1`,
          values: [name, value],
        }
      : {
          text: `INSERT INTO secret (name, value) VALUES ($1, $2)`,
          values: [name, value],
        };

    await this.db.query(query);
  }
}
