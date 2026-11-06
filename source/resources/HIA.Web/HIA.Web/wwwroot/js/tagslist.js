(function () {
    "use strict"

    var o = document, f = function (e) { return e && e.tagName ? e : o.getElementById(e) },
        v = function (d, c, t) { var e = o.createElement(d); if (c) { e.className = c } if (t) { e.innerHTML = t }; return e },
        at = function (e, g, v) { e = f(e); if (g && v) { e.setAttribute(g, v) } else { return e.getAttribute(g) } },
        g = function (d, c) { d = f(d); return d.getElementsByClassName(c) };

    // Plugin Constructor
    var TagsInput = function (opts) {
        this.options = Object.assign(TagsInput.defaults, opts);
        this.init();
    }

    // Initialize the plugin
    TagsInput.prototype.init = function (opts) {
        this.options = opts ? Object.assign(this.options, opts) : this.options;

        if (this.initialized)
            this.destroy();

        if (!(this.orignal_input = f(this.options.selector))) {
            console.error("tags-input couldn't find an element with the specified ID");
            return this;
        }

        this.arr = [];
        this.wrapper = v('div', 'tags-input-wrapper');
        this.input = v('input', 'tag-input');
        this.message = v('div', 'tag-error');
        this.resetr = v('a', 'tag-reset', '<span>&times;</span> Xóa hết');
        init(this);
        initEvents(this);

        this.initialized = true;
        return this;
    }

    // Add Tags for internal use
    function appendTag(plugin, tag) {
        plugin.arr.push(tag);
        plugin.arr.sort();

        var tagElm = v('span', 'tag', tag);
        var closr = v('a', 'tag-closer', '&times;');

        // delete the tag when icon is clicked
        closr.addEventListener('click', function (e) {
            e.preventDefault();
            var tagBox = this.parentNode;

            for (var i = 0; i < plugin.wrapper.childNodes.length; i++) {
                if (plugin.wrapper.childNodes[i] == tagBox) { plugin.deleteTag(tagBox, i) }
            }
        });

        tagElm.appendChild(closr);
        at(tagElm, 'hashtag', tag);

        var idx = plugin.arr.indexOf(tag), ins = plugin.message, alls = g(plugin.wrapper, 'tag');
        if (alls.length && alls.length > idx) { ins = alls[idx]; }
        plugin.wrapper.insertBefore(tagElm, ins);
        plugin.orignal_input.value = plugin.arr.join(plugin.options.splitchar);

        plugin.input.value = '';
        return plugin;
    }

    // Add Tags
    TagsInput.prototype.addTag = function (tag) {
        tag = tag.trim().toUpperCase();

        var err = this.anyErrors(tag);
        if (err && err != '') this.message.innerHTML = err;
        else appendTag(this, tag);
        return this;
    }

    // Delete Tags
    TagsInput.prototype.deleteTag = function (tag, i) {
        tag.remove();
        this.arr.splice(i, 1);
        this.orignal_input.value = this.arr.join(this.options.splitchar);
        return this;
    }

    function isOverride(tag, val) {
        if (val == tag) return true;

        if (tag.includes('*')) {
            tag = tag.substring(0, tag.length - 1);
            if (val.indexOf(tag) == 0) return true;
            if (tag[tag.length - 1] == '.') return (val == tag.substring(0, tag.length - 1));
        }
        else if (val.includes('*')) {
            return isOverride(val, tag);
        }

        return false;
    }

    // Make sure input string have no error with the plugin
    TagsInput.prototype.anyErrors = function (tag) {
        if (!tag) {
            return 'Mã không được để trống';
        }

        //Check amount of tag must be in range of max number
        if (this.options.max != null && this.arr.length >= this.options.max) {
            return 'Bạn chỉ được nhập tối đa ' + this.options.max + ' mã. Vui lòng xóa bớt';
        }

        //Check validation of format, must be A12.345.678 or A12.345.* or A12.34*
        if (this.options.pattern && this.options.pattern instanceof RegExp && !this.options.pattern.test(tag)) {
            return 'Mã [' + tag + '] không đúng định dạng. Vui lòng nhập lại';
        }

        //Check exists of tag to skip dublications
        if (!this.options.duplicate && this.arr.indexOf(tag) != -1) {
            var lst = g(this.wrapper, 'tag');
            for (var i = 0; i < lst.length; i++) {
                if (at(lst[i], 'hashtag') == tag) { lst[i].className = 'tag dub' }
            }

            return 'Mã [' + tag + '] đã tồn tại trong danh sách. Vui lòng nhập lại';
        }

        //Check whether new tag override old tag or reverse
        if (!this.options.override) {
            var lst = g(this.wrapper, 'tag'), err = 0;
            for (var i = 0; i < this.arr.length; i++) {
                if (isOverride(tag, this.arr[i])) {
                    if (lst.length > i && lst[i].className == 'tag') { lst[i].className = 'tag dub' }
                    err = 1;
                }
                else {
                    lst[i].className = 'tag';
                }
            }

            if (err) {
                return 'Mã [' + tag + '] có thể bao hàm hoặc bị bao hàm từ các mã đã có. Vui lòng xóa bỏ mã cũ đi';
            }
        }

        return '';
    }

    // Add tags programmatically
    TagsInput.prototype.addData = function (array) {
        var plugin = this;
        array.forEach(function (s) { plugin.addTag(s) })
        return this;
    }

    // Get the Input String
    TagsInput.prototype.getInputString = function () {
        return this.arr.join(this.options.splitchar);
    }

    // destroy the plugin
    TagsInput.prototype.destroy = function () {
        this.orignal_input.removeAttribute('hidden');

        delete this.orignal_input;
        var self = this;

        Object.keys(this).forEach(function (key) {
            if (self[key] instanceof HTMLElement)
                self[key].remove();

            if (key != 'options')
                delete self[key];
        });

        this.initialized = false;
    }

    // Private function to initialize the tag input plugin
    function init(tags) {
        tags.wrapper.append(tags.message);
        tags.wrapper.append(tags.input);
        tags.wrapper.append(tags.resetr);

        var arrs = tags.orignal_input.parentElement.getElementsByTagName('small');
        tags.submsg = (arrs && arrs.length) ? arrs[0] : v('small', 'hidden');
        tags.submsg.style.display = 'none';

        // Delete all tags when reset button is clicked
        tags.resetr.addEventListener('click', function (e) {
            e.preventDefault();
            tags.arr = [];
            tags.orignal_input.value = '';

            var lst = g(tags.wrapper, 'tag');
            while (lst.length) {
                lst[0].remove();
            }
        });

        tags.orignal_input.style.display = 'none';
        tags.orignal_input.parentNode.insertBefore(tags.wrapper, tags.orignal_input);

        var arrs = tags.orignal_input.value.split(tags.options.splitchar);
        tags.orignal_input.value = '';
        arrs.forEach(function (t) { t = (t || '').trim(); if (t) { appendTag(tags, t.trim().toUpperCase()) } });
    }

    // initialize the Events
    function initEvents(tags) {
        tags.wrapper.addEventListener('click', function () { tags.input.focus() });

        tags.oldval = '';
        tags.input.addEventListener('keydown', function (e) {
            tags.message.innerHTML = '';
            var lst = g(tags.wrapper, 'tag dub');
            for (var i = 0; i < lst.length; i++) {
                lst[i].className = 'tag';
            }
            if (!!(~[9, 13, 188].indexOf(e.keyCode))) {
                e.preventDefault();
                var arrs = tags.input.value.split(tags.options.splitchar);
                tags.addData(arrs);
                return false;
            }

            tags.oldval = tags.input.value;
            return true;
        });
        tags.input.addEventListener('focus', function (e) {
            tags.submsg.style.display = 'block';
        });
        tags.input.addEventListener('blur', function (e) {
            tags.submsg.style.display = 'none';
        });
    }

    // Set All the Default Values
    TagsInput.defaults = {
        selector: '',
        max: null,
        duplicate: false,
        override: false,
        pattern: /^([^\.\*\s]+\.)*[^\.\*\s]*(\*)?$/i,
        splitchar: ';'
    }

    window.TagsInput = TagsInput;

})();