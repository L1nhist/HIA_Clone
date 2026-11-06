using System.Linq.Expressions;
using HIA.Data.Base;

namespace HIA.Core.Base;
public class Evaluation<TOwner, TField>
    where TOwner : IRecord
{
    private readonly bool _empty;
    private bool _error;
    private bool _raise;
    private readonly TOwner _owner;
    private readonly TField _field;


    public Evaluation(TOwner owner, Expression<Func<TOwner, TField>> predicate)
    {
        _owner = owner;
        _field = owner == null ? default! : predicate.Compile().Invoke(owner);
        _error = false;
        _raise = false;
        _empty = owner == null || EvalHelper.IsEmpty(_field);
    }

    private static string Str(TField? value) => value?.ToString() ?? string.Empty;

    public static bool operator true(Evaluation<TOwner, TField> value) => value.Result();

    public static bool operator false(Evaluation<TOwner, TField> value) => !value.Result();

    public static bool operator !(Evaluation<TOwner, TField> value) => !value.Result();

    public Evaluation<TOwner, T> Field<T>(Expression<Func<TOwner, T>> expression)
    {
        return new Evaluation<TOwner, T>(_owner, expression)
        {
            _error = _error,
            _raise = _raise
        };
    }

    /// <summary>
    /// Kiểm tra xem có đúng điều kiện truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> Check(Expression<Func<TField, bool>> expression)
    {
        if (!_error && !_empty)
        {
            if (expression.Compile().Invoke(_field))
            {
                _error = true;
                _raise = true;
            }
        }
        return this;
    }

    /// <summary>
    /// Kiểm tra xem có đúng điều kiện truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> CheckIgnoreEmpty(Expression<Func<TField, bool>> expression)
    {
        if (!_error && _field != null)
        {
            if (expression.Compile().Invoke(_field))
            {
                _error = true;
                _raise = true;
            }
        }
        return this;
    }

    /// <summary>
    /// Kiểm tra xem có đúng điều kiện truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> Check(Expression<Func<TField, TField, bool>> expression, Evaluation<TOwner, TField> other)
    {
        if (!_error && !_empty && !other._error && !other._empty)
        {
            if (expression.Compile().Invoke(_field, other._field))
            {
                _error = true;
                _raise = true;
            }
        }
        return this;
    }

    /// <summary>
    /// Kiểm tra xem có không đúng điều kiện truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> CheckNot(Expression<Func<TField, bool>> expression)
    {
        if (!_error)
        {
            if (_empty || !expression.Compile().Invoke(_field))
            {
                _error = true;
                _raise = true;
            }
        }
        return this;
    }

    /// <summary>
    /// Kiểm tra xem có không đúng điều kiện truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> CheckNot(Expression<Func<TField, TField, bool>> expression, Evaluation<TOwner, TField> other)
    {
        if (!_error && !other._error)
        {
            if ((_empty && other._empty) || !expression.Compile().Invoke(_field, other._field))
            {
                _error = true;
                _raise = true;
            }
        }
        return this;
    }

    /// <summary>
    /// Báo lỗi theo mã truyền vào nếu đạt các điều kiện trước đó
    /// </summary>
    public Evaluation<TOwner, TField> ThenError(int error)
    {
        if (_raise)
        {
            _owner.RaiseError(error);
            _raise = false;
        }
        return this;
    }

    /// <summary>
    /// Báo lỗi theo mã truyền vào nếu không đạt các điều kiện trước đó
    /// </summary>
    public Evaluation<TOwner, TField> ElseError(int error)
    {
        if (!_error)
        {
            _owner.RaiseError(error);
            _error = true;
            _raise = false;
        }
        return this;
    }

    /// <summary>
    /// Trả về kết quả kiểm tra cuối cùng và thiết lập lại giá trị từ đầu
    /// (dùng khi muốn lập câu lệnh điều kiện if hoặc reset lại giá trị ban đầu)
    /// </summary>
    public bool Result()
    {
        var res = _error;
        _error = false;
        _raise = false;

        return res;
    }

    /// <summary>
    /// Kiểm tra xem giá trị truyền vào là null hoặc chuỗi rỗng hay không?
    /// </summary>
    public Evaluation<TOwner, TField> IsEmpty()
    {
        if (!_error && _empty)
        {
            _error = true;
            _raise = true;
        }
        return this;
    }

    /// <summary>
    /// Kiểm tra xem giá trị truyền vào không phải là null hoặc chuỗi rỗng hay không?
    /// </summary>
    public Evaluation<TOwner, TField> NotEmpty()
    {
        if (!_error && !_empty)
        {
            _error = true;
            _raise = true;
        }
        return this;
    }

    /// <summary>
    /// Nếu giá trị null hoặc chuỗi rỗng thì bỏ qua không thì sẽ thực hiện tiếp tục
    /// </summary>
    public Evaluation<TOwner, TField> IgnoreEmpty()
    {
        if (_error && _empty)
        {
            _error = false;
            _raise = false;
        }
        return this;
    }

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài đúng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthEqual(int min = 0)
        => Check(f => Str(f).Length == min);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài đúng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthEqual(Evaluation<TOwner, TField> other)
        => Check((f, o) => Str(f).Length == Str(o).Length, other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài không đúng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthNotEqual(int min = 0)
        => CheckNot(f => Str(f).Length == min);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài không đúng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthNotEqual(Evaluation<TOwner, TField> other)
        => CheckNot((f, o) => Str(f).Length == Str(o).Length, other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài lớn hơn giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthOver(int min = 0)
        => Check(f => Str(f).Length > min);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài lớn hơn giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthOver(Evaluation<TOwner, TField> other)
        => Check((f, o) => Str(f).Length > Str(o).Length, other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài lớn hơn hoặc bằng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthOverOrEqual(int min = 0)
        => Check(f => Str(f).Length >= min);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài lớn hơn giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthOverOrEqual(Evaluation<TOwner, TField> other)
        => Check((f, o) => Str(f).Length >= Str(o).Length, other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài nhỏ hơn giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthUnder(int max = 0)
        => Check(f => Str(f).Length < max);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài nhỏ hơn giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthUnder(Evaluation<TOwner, TField> other)
        => Check((f, o) => Str(f).Length < Str(o).Length, other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài nhỏ hơn hoặc bằng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthUnderOrEqual(int max = 0)
        => Check(f => Str(f).Length <= max);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài nhỏ hơn hoặc bằng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthUnderOrEqual(Evaluation<TOwner, TField> other)
        => Check((f, o) => Str(f).Length <= Str(o).Length, other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài trong khoảng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthBetween(int min = 0, int max = 0)
    {
        var l = Str(_field).Length;
        return Check(f => l > min && l < max);
    }

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài trong khoảng hoặc bằng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthBetweenOrEqual(int min = 0, int max = 0)
    {
        var l = Str(_field).Length;
        return Check(f => l >= min && l <= max);
    }

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài ngoài khoảng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthNotBetween(int min = 0, int max = 0)
    {
        var l = Str(_field).Length;
        return Check(f => l < min || l > max);
    }

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có độ dài ngoài khoảng hoặc bằng giá trị truyền vào hay không?
    /// </summary>
    public Evaluation<TOwner, TField> LengthNotBetweenOrEqual(int min = 0, int max = 0)
    {
        var l = Str(_field).Length;
        return Check(f => l <= min || l >= max);
    }

    /// <summary>
    /// Kiểm tra xem có chứa một trong các giá trị truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> HasValue(params TField[] values)
        => Check(f => values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có chứa một trong các giá trị truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> HasValue(IEnumerable<TField> values)
        => Check(f => values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có phải null, chuỗi rỗng hay chứa một trong các giá trị truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> HasValueOrEmpty(params TField[] values)
        => Check(f => _empty || values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có phải null, chuỗi rỗng hay chứa một trong các giá trị truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> HasValueOrEmpty(IEnumerable<TField> values)
        => Check(f => _empty || values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có không chứa một trong các giá trị truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> NotValue(params TField[] values)
        => CheckNot(f => values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có không chứa một trong các giá trị truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> NotValue(IEnumerable<TField> values)
        => CheckNot(f => values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có phải null, chuỗi rỗng hay không chứa một trong các giá trị truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> NotValueOrEmpty(params TField[] values)
        => CheckNot(f => _empty || values.Contains(f));

    /// <summary>
    /// Kiểm tra xem có phải null, chuỗi rỗng hay không chứa một trong các giá trị truyền vào không?
    /// </summary>
    public Evaluation<TOwner, TField> NotValueOrEmpty(IEnumerable<TField> values)
        => CheckNot(f => _empty || values.Contains(f));

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có chứa chuỗi truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> Contains(string value, bool ignoreCase = false)
        => Check(f => Str(f).Contains(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có chứa chuỗi truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> Contains(Evaluation<TOwner, TField> other, bool ignoreCase = false)
        => Check((f, o) => Str(f).Contains(Str(o), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal), other);

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có không chứa chuỗi truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> NotContains(string value, bool ignoreCase = false)
        => CheckNot(f => Str(f).Contains(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

    /// <summary>
    /// Kiểm tra xem dạng chuỗi của giá trị có không chứa chuỗi truyền vào không
    /// </summary>
    public Evaluation<TOwner, TField> NotContains(Evaluation<TOwner, TField> other, bool ignoreCase = false)
        => CheckNot((f, o) => Str(f).Contains(Str(o), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal), other);

    public Evaluation<TOwner, TField> QueryAny<T>(IEnumerable<T> values, Expression<Func<T, TField, bool>> predicate)
    {
        if (values == null || predicate == null) return this;

        var invoke = predicate.Compile();
        return Check(f => values.Any(v => invoke(v, f)));
    }

    public Evaluation<TOwner, TField> QueryNotAny<T>(IEnumerable<T> values, Expression<Func<T, TField, bool>> predicate)
    {
        if (values == null || predicate == null) return this;

        var invoke = predicate.Compile();
        return CheckNot(f => values.Any(v => invoke(v, f)));
    }
}
