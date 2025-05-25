const express = require('express');
const router = express.Router();

router.use('/verification', require('./verification'));
router.use('/user', require('./user'));

module.exports = router;
