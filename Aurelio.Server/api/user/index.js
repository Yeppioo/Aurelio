const express = require('express');
const router = express.Router();

router.use('/register', require('./register'));
router.use('/login', require('./login'));
router.use('/change-password', require('./change-password'));
router.use('/update-profile', require('./update-profile'));
router.use('/change-email', require('./change-email'));

module.exports = router; 