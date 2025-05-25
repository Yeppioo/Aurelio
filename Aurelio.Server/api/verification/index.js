const express = require('express');
const router = express.Router();

router.use('/send-code', require('./send-code'));

module.exports = router;
